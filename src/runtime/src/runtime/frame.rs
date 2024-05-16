use super::opcode::FuncId;

/// A frame which represents the state of the runtime
/// within a single invocation of a function.
#[derive(Debug)]
pub struct StackFrame {
    function: FuncId,
    is_implicit: bool,
    stack_start: usize,
    return_address: usize,
}

impl StackFrame {
    /// Creates a new stack frame.
    pub fn new(
        function: FuncId,
        stack_position: usize,
        is_implicit: bool,
        return_address: usize,
        arity: u32,
        locals_count: u32
    ) -> Self {
        let main_start = stack_position + arity as usize + locals_count as usize;
        
        Self {
            function,
            is_implicit,
            stack_start: stack_position,
            return_address
        }
    }

    /// Returns the function the stack frame represents an invocation of.
    pub fn function(&self) -> FuncId {
        self.function
    }

    /// Returns whether the stack frame is for an implicitly called function.
    pub fn is_implicit(&self) -> bool {
        self.is_implicit
    }

    /// Returns the stack position at the start of the stack frame.
    pub fn stack_start(&self) -> usize {
        self.stack_start
    }
    
    /// Returns the address to return to after the frame has finished.
    pub fn return_address(&self) -> usize {
        self.return_address
    }
}
