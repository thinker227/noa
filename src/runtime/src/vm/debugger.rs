use super::Vm;

/// User interface for debugging.
pub trait Debugger {
    /// Called by the VM when a debug breakpoint has been hit.
    fn debug_break(&mut self, vm: &mut Vm) -> DebugControlFlow;
}

/// Control flow from returning from a debug break.
pub enum DebugControlFlow {
    /// Continue vm execution as normal.
    Continue
}
