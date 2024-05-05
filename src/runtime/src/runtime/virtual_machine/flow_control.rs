use crate::runtime::opcode::FuncId;
use crate::runtime::frame::StackFrame;
use crate::runtime::exception::{Exception, ExceptionKind};
use crate::runtime::value::Value;

use super::VM;

impl VM {
    /// Enters a function.
    pub(super) fn enter_function(&mut self, id: FuncId) -> Result<(), Exception> {
        if self.call_stack.len() >= self.call_stack.capacity() {
            return Err(Exception::new(ExceptionKind::CallStackOverflow));
        }

        let function = self.functions.get(&id)
            .ok_or_else(|| Exception::new(ExceptionKind::InvalidFunction))?;
        
        // Push new stack frame onto the call stack
        
        let stack_position = self.stack_position();
        let arity = function.arity();
        let locals_count = function.locals_count();
        let frame = StackFrame::new(
            id,
            stack_position,
            arity,
            locals_count
        );

        self.call_stack.push(frame);

        // Push variables and parameters onto the stack

        for _ in 0..arity {
            self.push(Value::Nil)?;
        }

        for _ in 0..locals_count {
            self.push(Value::Nil)?;
        }

        Ok(())
    }

    /// Exits the current function.
    pub(super) fn exit_function(&mut self) {
        // It is impossible to be at this point
        // and for the call stack to be empty at the same time.
        let frame = self.call_stack.pop()
            .expect("call stack should not be empty");

        self.stack.truncate(frame.stack_start());
    }
}
