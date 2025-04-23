#![feature(try_blocks)]

use std::io::{self, Write};

use crossterm::event::{DisableMouseCapture, EnableMouseCapture};
use crossterm::execute;
use crossterm::terminal::{disable_raw_mode, enable_raw_mode, EnterAlternateScreen, LeaveAlternateScreen};
use noa_runtime::vm::Vm;
use noa_runtime::vm::debugger::{DebugControlFlow, Debugger};
use tui::backend::CrosstermBackend;
use tui::Terminal;
use noa_runtime::vm::debugger::{DebugControlFlow, DebugInspection, Debugger};

/// A debugger which provides a terminal user interface.
pub struct DebuggerTui<W: Write> {
    terminal: Terminal<CrosstermBackend<W>>
}

impl<W: Write> DebuggerTui<W> {
    /// Creates a new debugger TUI and initializes the terminal.
    pub fn new(buffer: W) -> Result<Self, io::Error> {
        let backend = CrosstermBackend::new(buffer);

        Ok(Self {
            terminal: Terminal::new(backend)?
        })
    }
}

impl<W: Write> Debugger for DebuggerTui<W> {
    fn init(&mut self) {
        let res: Result<(), io::Error> = try {
            enable_raw_mode()?;

            execute!(
                self.terminal.backend_mut(),
                EnterAlternateScreen,
                EnableMouseCapture
            )?;
        };

        if let Err(e) = res {
            eprintln!("An IO error occurred while initializing the debugger: {}", e);
        }
    }
    
    fn exit(&mut self) {
        let res: Result<(), io::Error> = try {
            disable_raw_mode()?;

            execute!(
                self.terminal.backend_mut(),
                LeaveAlternateScreen,
                DisableMouseCapture
            )?;

            self.terminal.show_cursor()?;
        };

        if let Err(e) = res {
            eprintln!("An IO error occurred while exiting the debugger: {e}");
        }
    }

    fn debug_break(&mut self, _: DebugInspection) -> DebugControlFlow {
        todo!()
    }
}
