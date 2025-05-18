use crate::heap::Heap;
use crate::vm::VmConsts;
use crate::vm::stack::Stack;
use crate::vm::frame::Frame;

/// User interface for debugging.
pub trait Debugger {
    // Initiates the debugging session.
    fn init(&mut self);

    /// Exits the debugging session.
    fn exit(&mut self);

    /// Called by the VM when a debug breakpoint has been hit.
    /// 
    /// The debugger is only permitted to access a read-only "inspection"
    /// snapshot of the VM's current state. Since the VM owns the debugger instance,
    /// a reference to the VM cannot be given out while the debugger is also being
    /// given out mutably to this method.
    fn debug_break(&mut self, inspection: DebugInspection) -> DebugControlFlow;
}

/// Debug inspection data from the VM.
pub struct DebugInspection<'vm> {
    pub consts: &'vm VmConsts,
    pub stack: &'vm Stack,
    pub heap: &'vm Heap,
    pub call_stack: &'vm Vec<Frame>,
    pub ip: usize,
}

/// Control flow from returning from a debug break.
pub enum DebugControlFlow {
    /// Continue vm execution as normal.
    Continue
}
