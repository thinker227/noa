use super::opcode::FuncId;

/// A frame which represents the state of the runtime
/// within a single invocation of a function.
#[derive(Debug)]
pub struct StackFrame {
    function: FuncId,
    ip: usize,
}

impl StackFrame {
    /// Creates a new stack frame.
    pub fn new(function: FuncId) -> Self {
        Self {
            function,
            ip: 0
        }
    }

    /// Returns the function the stack frame represents an invocation of.
    pub fn function(&self) -> FuncId {
        self.function
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
