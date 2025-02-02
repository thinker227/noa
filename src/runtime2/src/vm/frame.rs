use crate::native::NativeCall;
use crate::ark::FuncId;

/// A stack frame. Represents a single execution of a function.
pub struct Frame {
    /// The function the 
    pub function: FuncId,
    pub stack_start: usize,
    pub ret: FrameReturn,
    pub kind: FrameKind,
}

/// What to return to when a stack frame has finished.
pub enum FrameReturn {
    /// Return to an address within user code.
    User(usize),
    /// Return to a native call.
    Native(Box<dyn NativeCall>),
    /// Return to the root of execution in the virtual machine.
    /// Only the stack frame for the main function will have this frame return.
    ExecutionRoot,
}

/// The kind of a stack frame.
pub enum FrameKind {
    /// A stack frame for a call to a user function.
    UserFunction,
    /// A stack frame for a call to a native function.
    NativeFunction,
    /// A temporary stack frame used for block expressions.
    Temp {
        /// The index on the call stack where the parent function frame is located.
        parent_function_index: usize,
    },
}
