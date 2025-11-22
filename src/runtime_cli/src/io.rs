use noa_runtime::vm::{Input, Output};

pub struct StdInput {

}

impl StdInput {
    pub fn new() -> Self {
        StdInput {}
    }
}

impl Input for StdInput {
    fn read(&mut self, buf: &mut Vec<u8>) -> noa_runtime::vm::Result<()> {
        todo!()
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
    fn write(&mut self, bytes: &[u8]) -> noa_runtime::vm::Result<()> {
        todo!()
    }
}
