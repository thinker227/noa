use super::{function::Function, opcode::Opcode};

/// A frame which represents the state of the runtime
/// within a single invocation of a function.
#[derive(Debug)]
pub struct StackFrame<'a> {
    function: &'a Function,
    ip: usize,
}

impl<'a> StackFrame<'a> {
    /// Creates a new stack frame.
    pub fn new(function: &'a Function) -> Self {
        Self {
            function,
            ip: 0
        }
    }

    /// Returns the function the stack frame represents an invocation of.
    pub fn function(&self) -> &Function {
        self.function
    }

    /// Returns the current op-code, if the stack frame hasn't finished.
    pub fn current(&self) -> Option<Opcode> {
        self.function.code().get(self.ip).copied()
    }

    /// Progresses to the next op-code, if the stack frame hasn't finished.
    pub fn progress(&mut self) -> Option<Opcode> {
        let opcode = self.current()?;
        self.ip += 1;
        Some(opcode)
    }

    /// Sets the instruction pointer.
    pub fn set_ip(&mut self, ip: usize) {
        self.ip = ip;
    }

    /// Returns whether the stack frame has finished or not.
    pub fn is_finished(&self) -> bool {
        self.ip < self.function.code().len()
    }
}
