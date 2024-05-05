use super::opcode::FuncId;

/// A frame which represents the state of the runtime
/// within a single invocation of a function.
#[derive(Debug)]
pub struct StackFrame {
    function: FuncId,
    is_implicit: bool,
    stack_start: usize,
    main_start: usize,
    ip: usize,
}

impl StackFrame {
    /// Creates a new stack frame.
    pub fn new(function: FuncId, stack_position: usize, is_implicit: bool, arity: u32, locals_count: u32) -> Self {
        let main_start = stack_position + arity as usize + locals_count as usize;
        
        Self {
            function,
            is_implicit,
            stack_start: stack_position,
            main_start,
            ip: 0
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

    /// Increments the instruction pointer and returns the previous value.
    pub fn increment_ip(&mut self) -> usize {
        let x = self.ip;
        self.ip += 1;
        x
    }

    /// Sets the instruction pointer.
    pub fn set_ip(&mut self, ip: usize) {
        self.ip = ip;
    }
}
