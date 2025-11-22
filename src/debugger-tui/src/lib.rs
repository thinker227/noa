#![feature(try_blocks)]
#![allow(clippy::new_without_default)]

use std::{cell::RefCell, io, rc::Rc};

use crossterm::{
    event::{
        self,
        Event,
        KeyEvent,
        KeyCode
    },
    execute,
    terminal::{
        enable_raw_mode,
        disable_raw_mode,
        EnterAlternateScreen,
        LeaveAlternateScreen
    }
};
use ratatui::{
    prelude::CrosstermBackend, Frame, Terminal
};
use noa_runtime::vm::{Input, Output, debugger::{
    DebugControlFlow,
    DebugInspection,
    Debugger
}};
use state::State;
use widgets::MainWidget;

mod state;
mod instruction;
mod widgets;
mod utils;

/// Sets a panic hook that restores the terminal before panicking.
/// 
/// Copied from Ratatui's internal `set_panic_hook`.
fn set_panic_hook() {
    let hook = std::panic::take_hook();
    std::panic::set_hook(Box::new(move |info| {
        restore_terminal();
        hook(info);
    }));
}

/// Initializes the terminal.
/// 
/// Copied from [`ratatui::try_init`].
fn init_terminal() {
    color_eyre::install()
        .expect("failed to install color_eyre");

    set_panic_hook();

    let res: Result<(), io::Error> = try {
        enable_raw_mode()?;
        execute!(
            io::stdout(),
            EnterAlternateScreen
        )?;
    };

    res.expect("failed to initialize terminal");
}

/// Restore the terminal to its original state.
/// 
/// Copied from [`ratatui::try_restore`].
fn restore_terminal() {
    let res: Result<(), io::Error> = try {
        disable_raw_mode()?;
        execute!(
            io::stdout(),
            LeaveAlternateScreen
        )?;
    };

    res.expect("failed to restore terminal");
}

/// A debugger which provides a terminal user interface.
pub struct DebuggerTui {
    terminal: Terminal<CrosstermBackend<io::Stdout>>,
    output_buf: Rc<RefCell<Vec<u8>>>,
    state: State
}

impl DebuggerTui {
    /// Creates a new debugger TUI and initializes the terminal.
    pub fn new() -> Result<Self, io::Error> {
        let terminal = Terminal::new(CrosstermBackend::new(io::stdout()))?;

        Ok(Self {
            terminal,
            output_buf: Rc::new(RefCell::new(Vec::new())),
            state: State::default(),
        })
    }

    pub fn output_buf(&self) -> Rc<RefCell<Vec<u8>>> {
        self.output_buf.clone()
    }
}

impl Debugger for DebuggerTui {
    fn init(&mut self) {
        init_terminal();
    }
    
    fn exit(&mut self) {
        restore_terminal();
    }

    fn debug_break(&mut self, inspection: DebugInspection) -> DebugControlFlow {
        adjust_state(&inspection, &mut self.state);

        loop {
            self.terminal.draw(|frame| draw(&self.state, &inspection, frame))
                .expect("failed to render");

            match event::read() {
                Ok(event) => {
                    let result = handle_event(event, &mut self.state);

                    match result {
                        EventHandleResult::Continue => {},
                        EventHandleResult::Exit => break,
                    }
                },
                Err(e) => {
                    eprintln!("failed to read input: {e}");
                    break;
                },
            }
        }

        DebugControlFlow::Continue
    }
}

fn adjust_state(_: &DebugInspection, _: &mut State) {
    
}

enum EventHandleResult {
    Continue,
    Exit,
}

fn handle_event(event: Event, _state: &mut State) -> EventHandleResult {
    if let Event::Key(key_event) = event && let KeyEvent { code: KeyCode::Char(' '), .. } = key_event {
        return EventHandleResult::Exit;
    }

    EventHandleResult::Continue
}

fn draw(state: &State, inspection: &DebugInspection, frame: &mut Frame) {
    let main_widget = MainWidget {
        inspection,
        _state: state
    };

    frame.render_widget(main_widget, frame.area());
}

pub struct DebugInput {

}

impl DebugInput {
    pub fn new() -> Self {
        Self {}
    }
}

impl Input for DebugInput {
    fn read(&mut self, buf: &mut Vec<u8>) -> noa_runtime::vm::Result<()> {
        todo!()
    }
}

pub struct DebugOutput {
    buf: Rc<RefCell<Vec<u8>>>,
}

impl DebugOutput {
    pub fn new(buf: Rc<RefCell<Vec<u8>>>) -> Self {
        Self {
            buf
        }
    }
}

impl Output for DebugOutput {
    fn write(&mut self, bytes: &[u8]) -> noa_runtime::vm::Result<()> {
        let mut borrow = self.buf.borrow_mut();
        borrow.extend_from_slice(bytes);
        
        Ok(())
    }
}
