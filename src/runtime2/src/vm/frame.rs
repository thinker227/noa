use crate::ark::FuncId;

/// A stack frame. Represents a single execution of a function.
pub struct Frame {
    /// The function the frame is for an execution of.
    pub function: FuncId,
    /// The index which marks the start of the frame's allocated space on the stack.
    pub stack_start: usize,
    /// The bytecode address to return to once execution of the function has finished.
    /// Is only [`None`] if the frame is the bottom-most frame.
    pub ret: Option<usize>,
    /// The kind of the frame.
    pub kind: FrameKind,
}

/// The kind of a stack frame.
pub enum FrameKind {
    /// The frame is for a call to a user function.
    UserFunction,
    /// The frame is for a call to a native function.
    NativeFunction,
    /// The frame is a temporary stack frame used for block expressions.
    Temp {
        /// The index on the call stack where the parent user function frame is located.
        parent_function_index: usize,
    },
}
