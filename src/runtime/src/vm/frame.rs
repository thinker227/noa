use crate::ark::opcode::{Address, FuncId};

/// A frame on the call stack.
#[derive(Debug, Clone)]
pub enum StackFrame {
    /// A function call.
    Function(FunctionStackFrame),
    /// A temporary stack frame.
    Temporary {
        /// The function stack frame the temporary frame is in.
        function: FunctionStackFrame,
        /// The position on the value stack where the frame begins.
        stack_start: usize,
    }
}

/// A stack frame which represents a function call.
#[derive(Debug, Clone)]
pub struct FunctionStackFrame {
    pub function: FuncId,
    pub stack_start: usize,
    pub call: Call,
    pub caller: Caller,
}

/// Info about the call which produced a stack frame,
/// i.e. the call which produced a stack frame.
#[derive(Debug, Clone)]
pub struct Call {
    /// Whether the call is explicit (i.e. from user code) or implicit (i.e. from runtime code).
    pub is_implicit: bool,
}

/// Info about the caller of a stack frame,
/// i.e. the code from which the call was which produced the stack frame.
#[derive(Debug, Clone)]
pub enum Caller {
    /// The caller is user code.
    Code {
        /// The address to return to once the frame has finished.
        return_address: Address,
        /// The address of the caller.
        caller_address: Address,
    },
    /// The caller is runtime code.
    Runtime,
}

impl StackFrame {
    /// Returns the current stack frame as a [FunctionStackFrame].
    pub fn as_function(&self) -> &FunctionStackFrame {
        match self {
            StackFrame::Function(f) => f,
            StackFrame::Temporary { function, .. } => function,
        }
    }

    /// The function the stack frame represents an invocation of.
    pub fn function(&self) -> FuncId {
        self.as_function().function
    }

    /// The stack position at the start of the stack frame.
    pub fn stack_start(&self) -> usize {
        match self {
            StackFrame::Function(f) => f.stack_start,
            StackFrame::Temporary { stack_start, .. } => *stack_start,
        }
    }

    /// Whether the call is explicit (i.e. from user code) or implicit (i.e. from runtime code).
    pub fn call_is_implicit(&self) -> bool {
        self.as_function().call.is_implicit
    }
    
    /// The address to return to once the frame has finished.
    /// 
    /// Returns [`Some`] is the stack frame has a return address,
    /// otherwise [`None`] in case the caller is implicit.
    pub fn return_address(&self) -> Option<Address> {
        self.as_function().return_address()
    }

    /// The address of the caller.
    /// 
    /// Returns [`Some`] is the stack frame has a caller address,
    /// otherwise [`None`] in case the caller is implicit.
    pub fn caller_address(&self) -> Option<Address> {
        self.as_function().caller_address()
    }
}

impl FunctionStackFrame {
    /// The address to return to once the frame has finished.
    /// 
    /// Returns [`Some`] is the stack frame has a return address,
    /// otherwise [`None`] in case the caller is implicit.
    pub fn return_address(&self) -> Option<Address> {
        match self.caller {
            Caller::Code { return_address, .. } => Some(return_address),
            Caller::Runtime => None,
        }
    }

    /// The address of the caller.
    /// 
    /// Returns [`Some`] is the stack frame has a caller address,
    /// otherwise [`None`] in case the caller is implicit.
    pub fn caller_address(&self) -> Option<Address> {
        match self.caller {
            Caller::Code { caller_address, .. } => Some(caller_address),
            Caller::Runtime => None,
        }
    }
}
