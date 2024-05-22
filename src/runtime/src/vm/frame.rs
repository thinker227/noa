use crate::ark::opcode::{Address, FuncId};

/// A frame on the call stack which represents the call of a single function.
#[derive(Debug)]
pub struct StackFrame {
    pub function: FuncId,
    pub stack_start: usize,
    pub call: Call,
    pub caller: Caller,
}

/// Info about the call which produced a stack frame,
/// i.e. the call which produced a stack frame.
#[derive(Debug)]
pub struct Call {
    /// Whether the call is explicit (i.e. from user code) or implicit (i.e. from runtime code).
    pub is_implicit: bool,
}

/// Info about the caller of a stack frame,
/// i.e. the code from which the call was which produced the stack frame.
#[derive(Debug)]
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
    /// Creates a new stack frame.
    pub fn new(
        function: FuncId,
        stack_position: usize,
        call: Call,
        caller: Caller
    ) -> Self {        
        Self {
            function,
            stack_start: stack_position,
            call,
            caller
        }
    }

    /// The function the stack frame represents an invocation of.
    pub fn function(&self) -> FuncId {
        self.function
    }

    /// The stack position at the start of the stack frame.
    pub fn stack_start(&self) -> usize {
        self.stack_start
    }

    /// Whether the call is explicit (i.e. from user code) or implicit (i.e. from runtime code).
    pub fn call_is_implicit(&self) -> bool {
        self.call.is_implicit
    }
    
    /// The address to return to once the frame has finished.
    /// 
    /// Returns [`Some`] is the stack frame has a return address,
    /// otherwise [`None`] in case the caller is implicit.
    pub fn return_address(&self) -> Option<Address> {
        match self.caller {
            Caller::Code { return_address, caller_address } => Some(return_address),
            Caller::Runtime => None,
        }
    }

    /// The address of the caller.
    /// 
    /// Returns [`Some`] is the stack frame has a caller address,
    /// otherwise [`None`] in case the caller is implicit.
    pub fn caller_address(&self) -> Option<Address> {
        match self.caller {
            Caller::Code { return_address, caller_address } => Some(caller_address),
            Caller::Runtime => None,
        }
    }
}
