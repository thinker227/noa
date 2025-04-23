use noa_runtime::vm::Vm;
use noa_runtime::vm::debugger::{DebugControlFlow, Debugger};

/// A debugger which provides a terminal user interface.
pub struct DebuggerTui {

}

impl DebuggerTui {
    pub fn new() -> Self {
        Self {}
    }
}

impl Debugger for DebuggerTui {
    fn debug_break(&mut self, _: &mut Vm) -> DebugControlFlow {
        todo!()
    }
}
