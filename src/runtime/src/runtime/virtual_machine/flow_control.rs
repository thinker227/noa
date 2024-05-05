use crate::runtime::opcode::FuncId;
use crate::runtime::frame::StackFrame;
use crate::runtime::exception::{Exception, ExceptionKind};
use crate::runtime::value::Value;

use super::VM;

impl VM {
    /// Enters a function.
    pub(super) fn call(&mut self, id: FuncId, arg_count: u32, is_implicit: bool) -> Result<(), Exception> {
        if self.call_stack.len() >= self.call_stack.capacity() {
            return Err(Exception::new(ExceptionKind::CallStackOverflow));
        }

        let function = self.functions.get(&id)
            .ok_or_else(|| Exception::new(ExceptionKind::InvalidFunction))?;

        let arity = function.arity();
        let locals_count = function.locals_count();

        // Get rid of any additional arguments to the function
        // outside of what the function expects
        if arg_count > arity {
            for _ in 0..(arg_count - arity) {
                self.pop()?;
            }
        }
        
        // Fill out the stack with missing arguments
        // if there are not enough arguments
        if arg_count < arity {
            for _ in arg_count..arity {
                self.push(Value::Nil)?;
            }
        }

        // This is a kinda funny hack because parameters occupy the very start of
        // the function's allocated portion of the stack, so the stack frame's
        // stack start position is set to the start of where the arguments were pushed
        // since the arguments will have been pushed in order onto the stack.
        // These arguments will be popped off the stack at the end of the function,
        // which is functionally no different from how they would act if the arguments
        // were popped before being passed as arguments.
        let frame_stack_start_position = self.stack_position() - arity as usize;

        // Push placeholder values for variables onto the stack
        for _ in 0..locals_count {
            self.push(Value::Nil)?;
        }

        // Push new stack frame onto the call stack
        self.call_stack.push(StackFrame::new(
            id,
            frame_stack_start_position,
            is_implicit,
            arity,
            locals_count
        ));

        Ok(())
    }

    /// Exits the current function.
    pub(super) fn ret(&mut self) {
        // It is impossible to be at this point
        // and for the call stack to be empty at the same time.
        let frame = self.call_stack.pop()
            .expect("call stack should not be empty");

        let stack_start = frame.stack_start();

        let stack_backtrack_position = if frame.is_implicit() {
            // If the function was called implicitly then just the arguments need to be popped.
            stack_start
        } else {
            // If the function was not called implicitly then there's a function value
            // sitting just below the arguments on the stack, which also needs to be popped.
            stack_start - 1
        };

        self.stack.truncate(stack_backtrack_position);
    }
}
