use crate::ark::opcode::FuncId;
use crate::vm::frame::{Call, Caller, StackFrame};
use crate::runtime::exception::{ExceptionData, VMException};
use crate::runtime::value::Value;

use super::frame::FunctionStackFrame;
use super::VM;

impl VM {
    /// Enters a function.
    /// 
    /// # Arguments
    /// * `id` - The ID of the function to call.
    /// * `arg_count` - The amount of argument the function is called with.
    /// * `call_is_implicit` - Whether the call is explicit (i.e. from user code) or implicit (i.e. from runtime code.)
    /// * `caller` - The current caller.
    pub(super) fn call(
        &mut self,
        id: FuncId,
        arg_count: u32,
        call_is_implicit: bool,
        caller: Caller,
    ) -> Result<(), ExceptionData> {
        self.check_stack_overflow()?;

        let function = self.functions.get(&id)
            .ok_or(ExceptionData::VM(VMException::InvalidFunction))?;

        let arity = function.arity();
        let locals_count = function.locals_count();
        let address = function.address();

        // Get rid of any additional arguments to the function
        // outside of what the function expects
        if arg_count > arity {
            for _ in 0..(arg_count - arity) {
                self.stack.pop()?;
            }
        }
        
        // Fill out the stack with missing arguments
        // if there are not enough arguments
        if arg_count < arity {
            for _ in arg_count..arity {
                self.stack.push(Value::Nil)?;
            }
        }

        // This is a kinda funny hack because parameters occupy the very start of
        // the function's allocated portion of the stack, so the stack frame's
        // stack start position is set to the start of where the arguments were pushed
        // since the arguments will have been pushed in order onto the stack.
        // These arguments will be popped off the stack at the end of the function,
        // which is functionally no different from how they would act if the arguments
        // were popped before being passed as arguments.
        let frame_stack_start_position = self.stack.head_position() - arity as usize;

        // Push placeholder values for variables onto the stack
        for _ in 0..locals_count {
            self.stack.push(Value::Nil)?;
        }

        // Push new stack frame onto the call stack
        let call = Call {
            is_implicit: call_is_implicit
        };
        self.call_stack.push(StackFrame::Function(FunctionStackFrame {
            function: id,
            stack_start: frame_stack_start_position,
            call,
            caller
        }));

        self.code.jump(address.value());

        Ok(())
    }

    /// Returns from the current function.
    pub(super) fn ret(&mut self) {
        // It is impossible to be at this point
        // and for the call stack to be empty at the same time.
        let frame = self.call_stack.pop()
            .expect("call stack should not be empty");

        // Make sure to use a function stack frame.
        let frame = frame.as_function();

        let stack_start = frame.stack_start;

        let stack_backtrack_position = if frame.call.is_implicit {
            // If the function was called implicitly then just the arguments need to be popped.
            stack_start
        } else {
            // If the function was not called implicitly then there's a function value
            // sitting just below the arguments on the stack, which also needs to be popped.
            stack_start - 1
        };

        self.stack.clear_to(stack_backtrack_position);

        // We can only return somewhere if there is somewhere to return, quite obviously.
        // If the stack frame caller was implicit then there is nowhere to return to.
        if let Some(return_address) = frame.return_address() {
            self.code.jump(return_address.value());
        }
    }

    pub fn enter_temp_frame(&mut self) -> Result<(), ExceptionData> {
        self.check_stack_overflow()?;

        let function_frame = self.call_stack.last()
            .expect("call stack should not be empty")
            .as_function()
            .clone();

        let frame_stack_start_position = self.stack.head_position();

        let frame = StackFrame::Temporary {
            function: function_frame,
            stack_start: frame_stack_start_position
        };

        self.call_stack.push(frame);

        Ok(())
    }

    pub fn exit_stack_frame(&mut self) {
        let function_frame = self.call_stack.last()
            .expect("call stack should not be empty");

        // If the current stack frame is not a temporary stack frame then there is an error in the bytecode.
        // Todo: perhaps return an exception if this is the case.
        let stack_backtrack_position = match function_frame {
            StackFrame::Temporary { stack_start, .. } => *stack_start,
            _ => panic!("cannot exit temporary stack frame when the current stack frame is not temporary"),
        };

        self.stack.clear_to(stack_backtrack_position);

        // Since we've already checked whether the stack contains a last element
        // then it is impossible for this to return an error.
        self.call_stack.pop().unwrap();
    }

    #[must_use]
    fn check_stack_overflow(&self) -> Result<(), ExceptionData> {
        if self.call_stack.len() >= self.call_stack.capacity() {
            Err(ExceptionData::VM(VMException::CallStackOverflow))
        } else {
            Ok(())
        }
    }
}
