#![feature(try_blocks)]

use std::io;

use crossterm::{
    execute,
    terminal::{
        enable_raw_mode,
        disable_raw_mode,
        EnterAlternateScreen,
        LeaveAlternateScreen
    }
};
use ratatui::{
    prelude::CrosstermBackend,
    widgets::Widget,
    Frame,
    Terminal
};
use noa_runtime::vm::debugger::{
    DebugControlFlow,
    DebugInspection,
    Debugger
};

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
    state: State
}

struct State {
    exit: bool
}

impl DebuggerTui {
    /// Creates a new debugger TUI and initializes the terminal.
    pub fn new() -> Result<Self, io::Error> {
        let terminal = Terminal::new(CrosstermBackend::new(io::stdout()))?;

        Ok(Self {
            terminal,
            state: State {
                exit: false,
            },
        })
    }
}

impl Debugger for DebuggerTui {
    fn init(&mut self) {
        init_terminal();
    }
    
    fn exit(&mut self) {
        restore_terminal();
    }

    fn debug_break(&mut self, _: DebugInspection) -> DebugControlFlow {
        while !self.state.exit {
            self.terminal.draw(|frame| draw(&self.state, frame))
                .expect("failed to render");
        }

        DebugControlFlow::Continue
    }
}

fn draw(state: &State, frame: &mut Frame) {
    frame.render_widget(state, frame.area());
}

impl Widget for &State {
    fn render(self, _: ratatui::prelude::Rect, _: &mut ratatui::prelude::Buffer) {
        todo!()
    }
}
