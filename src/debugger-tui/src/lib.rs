use std::io::{self, Write};

use crossterm::event::{DisableMouseCapture, EnableMouseCapture};
use crossterm::execute;
use crossterm::terminal::{disable_raw_mode, enable_raw_mode, EnterAlternateScreen, LeaveAlternateScreen};
use noa_runtime::vm::Vm;
use noa_runtime::vm::debugger::{DebugControlFlow, Debugger};
use tui::backend::CrosstermBackend;
use tui::Terminal;

/// A debugger which provides a terminal user interface.
pub struct DebuggerTui<W: Write> {
    terminal: Terminal<CrosstermBackend<W>>
}

impl<W: Write> DebuggerTui<W> {
    /// Creates a new debugger TUI and initializes the terminal.
    pub fn init(buffer: W) -> Result<Self, io::Error> {
        let mut backend = CrosstermBackend::new(buffer);

        enable_raw_mode()?;
        execute!(
            backend,
            EnterAlternateScreen,
            EnableMouseCapture
        )?;

        Ok(Self {
            terminal: Terminal::new(backend)?
        })
    }

    fn exit(&mut self) -> Result<(), io::Error> {
        disable_raw_mode()?;

        execute!(
            self.terminal.backend_mut(),
            LeaveAlternateScreen,
            DisableMouseCapture
        )?;

        self.terminal.show_cursor()?;

        Ok(())
    }
}

impl<W: Write> Debugger for DebuggerTui<W> {
    fn debug_break(&mut self, _: &mut Vm) -> DebugControlFlow {
        todo!()
    }
}
