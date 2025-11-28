use std::io::{self, Write};

use noa_runtime::{exception::Exception, vm::{Input, Output}};

pub struct StdInput {

}

impl StdInput {
    pub fn new() -> Self {
        StdInput {}
    }
}

impl Input for StdInput {
    fn read(&mut self, buf: &mut Vec<u8>) -> Result<(), Exception> {
        let mut str = String::new();
        io::stdin().read_line(&mut str)
            .map_err(|_| Exception::Custom(
                String::from("failed to read from stdin")
            ))?;
        
        str.pop(); // Push trailing newline character.

        buf.extend_from_slice(str.as_bytes());
        
        Ok(())
    }
}

pub struct StdOutput {

}

impl StdOutput {
    pub fn new() -> Self {
        Self {}
    }
}

impl Output for StdOutput {
    fn write(&mut self, bytes: &[u8]) -> Result<(), Exception> {
        io::stdout().write_all(bytes)
            .map_err(|_| Exception::Custom(
                String::from("failed to write to stdout")
            ))
    }
}
