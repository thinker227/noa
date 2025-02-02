use crate::ark::FuncId;
use crate::exception::Exception;
use crate::opcode;
use crate::value::Value;
use crate::vm::frame::{Frame, FrameKind, FrameReturn};

use super::Vm;

impl Vm {
    pub fn execute(&mut self, _function: FuncId, _args: &Vec<Value>) {
        todo!()
    }

    fn _call_from_user(&mut self, function: FuncId, arg_count: u32) -> Result<(), Exception> {
        if function.is_native() {
            self._call_native(function, arg_count)
        } else {
            self._call_user(function, arg_count)
        }
    }

    fn _call_user(&mut self, id: FuncId, arg_count: u32) -> Result<(), Exception> {
        let user_index = id.decode();
        let function = self.consts.functions.get(user_index as usize)
            .ok_or_else(|| Exception::InvalidUserFunction(user_index))?;

        let arity = function.arity;
        let locals_count = function.locals_count;
        let address = function.address as usize;

        // Get rid of any additional arguments outside of what the function expects.
        if arg_count > arity {
            for _ in arity..arg_count {
                self.stack.pop()?;
            }
        }

        // Fill out the stack with missing arguments if there are not enough.
        if arg_count < arity {
            for _ in arg_count..arity {
                self.stack.push(Value::Nil)?;
            }
        }

        // Fun hack!
        // The function arguments occupy the very bottom of the stack space for the function,
        // meaning that, since the arguments are already on the stack in the correct order,
        // we can just set the frame's stack start index to the current stack head
        // minus the function's arity.
        let stack_start = self.stack.head() - arity as usize;

        // Push values onto the stack as placeholders for the function's locals.
        // These will later be overridden once the locals are assigned.
        for _ in 0..locals_count {
            self.stack.push(Value::Nil)?;
        }

        let ret = match self.call_stack.last() {
            Some(frame) => match &frame.kind {
                // Return back to the current instruction pointer.
                FrameKind::UserFunction | FrameKind::Temp { .. } =>
                    FrameReturn::User(self.ip),
                
                // There is no way for user functions to be called from native functions like this.
                // Native functions have their own special mechanism for calling user/native functions
                // which is completely unrelated. This is why `call_from_user` is named "call from user".
                FrameKind::NativeFunction =>
                    panic!("user functions are not callable from native functions using `call_from_user`"),
            },
            // If there is no current stack frame, that means we're going to return to the execution root,
            // i.e. that it is the main function (or some variety of it) that's being called.
            None => FrameReturn::ExecutionRoot,
        };

        let frame = Frame {
            function: id,
            stack_start,
            ret,
            kind: FrameKind::UserFunction,
        };

        self.call_stack.push_within_capacity(frame)
            .map_err(|_| Exception::CallStackOverflow)?;

        self.ip = address;

        Ok(())
    }

    fn _call_native(&mut self, id: FuncId, _arg_count: u32) -> Result<(), Exception> {
        let native_index = id.decode();
        let _function = self.consts.native_functions.get(native_index as usize)
            .ok_or_else(|| Exception::InvalidNativeFunction(native_index))?;

        todo!()
    }

    /// Runs the interpreter indefinitely until an exception occurs, or the call stack runs out.
    fn _run(&mut self) -> Result<(), Exception> {
        while !self.call_stack.is_empty() {
            self._interpret_instruction()?;
        }

        Ok(())
    }

    /// Interprets the current instruction pointed to by [`Self::ip`].
    fn _interpret_instruction(&mut self) -> Result<(), Exception> {
        let address = self.ip;
        let opcode = self.consts.code.get(address)
            .ok_or(Exception::Overrun)?;

        match *opcode {
            opcode::NO_OP => {},

            opcode::JUMP => todo!(),

            opcode::JUMP_IF => todo!(),

            opcode::CALL => todo!(),

            opcode::RET => todo!(),

            opcode::ENTER_TEMP_FRAME => todo!(),

            opcode::EXIT_TEMP_FRAME => todo!(),

            opcode::PUSH_INT => todo!(),

            opcode::PUSH_BOOL => todo!(),

            opcode::PUSH_FUNC => todo!(),

            opcode::PUSH_NIL => todo!(),

            opcode::POP => todo!(),

            opcode::DUP => todo!(),

            opcode::SWAP => todo!(),

            opcode::STORE_VAR => todo!(),

            opcode::LOAD_VAR => todo!(),

            opcode::ADD => todo!(),

            opcode::SUB => todo!(),

            opcode::MULT => todo!(),

            opcode::DIV => todo!(),

            opcode::EQUAL => todo!(),

            opcode::LESS_THAN => todo!(),

            opcode::NOT => todo!(),

            opcode::AND => todo!(),

            opcode::OR => todo!(),

            opcode::GREATER_THAN => todo!(),

            opcode::BOUNDARY => return Err(Exception::Overrun),

            _ => return Err(Exception::UnknownOpcode(*opcode))
        }

        self.ip += 1;

        Ok(())
    }
}
