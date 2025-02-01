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
}

/// The kind of a stack frame.
pub enum FrameKind {
    /// A stack frame for a call of a function.
    Function,
    /// A temporary stack frame used for block expressions.
    Temp,
}
