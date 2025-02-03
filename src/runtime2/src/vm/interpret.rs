use std::cell::RefCell;
use std::mem;

use crate::ark::FuncId;
use crate::exception::Exception;
use crate::native::{NativeCall, NativeCallArgs, NativeCallControlFlow};
use crate::opcode;
use crate::value::{Closure, Value};
use crate::vm::frame::{Frame, FrameKind, FrameReturn};

use super::{Vm, Ip};

impl Vm<'_> {
    /// Calls a function with a specified amount of arguments from the stack.
    fn _call(&mut self, function: FuncId, arg_count: u32) -> Result<(), Exception> {
        if function.is_native() {
            self._call_native(function, arg_count)
        } else {
            self._call_user(function, arg_count)
        }
    }

    /// Calls a closure with a specified amount of arguments from the stack.
    fn _call_closure(&mut self, _closure: Closure, _arg_count: u32) -> Result<(), Exception> {
        todo!()
    }

    /// Calls a user function.
    fn _call_user(&mut self, id: FuncId, arg_count: u32) -> Result<(), Exception> {
        let user_index = id.decode();
        let function = self.consts.functions.get(user_index as usize)
            .ok_or_else(|| Exception::InvalidUserFunction(user_index))?;

        let arity = function.arity;
        let locals_count = function.locals_count;
        let address = function.address as usize;
        let ret = self._replace_ip_into_frame_return(Ip::User(address));

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

        let frame = Frame {
            function: id,
            stack_start,
            ret,
            kind: FrameKind::UserFunction,
        };

        self.call_stack.stack.push_within_capacity(frame)
            .map_err(|_| Exception::CallStackOverflow)?;

        Ok(())
    }

    /// Calls a native function.
    fn _call_native(&mut self, id: FuncId, arg_count: u32) -> Result<(), Exception> {
        let native_index = id.decode();
        let function = self.consts.native_functions.get(native_index as usize)
            .ok_or_else(|| Exception::InvalidNativeFunction(native_index))?;

        let stack_start = self.stack.head() - arg_count as usize;

        let args = self.stack.slice_from_end(arg_count as usize)
            .ok_or(Exception::StackUnderflow)?;

        let call: Box<RefCell<dyn NativeCall>> = function(args);

        let ret = self._replace_ip_into_frame_return(Ip::Native(call));

        let frame = Frame {
            function: id,
            stack_start,
            ret,
            kind: FrameKind::NativeFunction,
        };

        self.call_stack.stack.push_within_capacity(frame)
            .map_err(|_| Exception::CallStackOverflow)?;

        todo!()
    }

    /// Replaces the current instruction pointer with a new one,
    /// and constructs a [`FrameReturn`] from the old instruction pointer.
    fn _replace_ip_into_frame_return(&mut self, new_ip: Ip) -> FrameReturn {
        let old_ip = mem::replace(&mut self.ip, new_ip);
        match old_ip {
            Ip::User(adr) => FrameReturn::User(adr),
            Ip::Native(native_call) => FrameReturn::Native(native_call),
            Ip::None => FrameReturn::ExecutionRoot,
        }
    }

    /// Returns from a native function.
    fn _return_from_native(&mut self, _value: Value) {
        todo!()
    }

    /// Runs the interpreter indefinitely until an exception occurs, or the call stack runs out.
    fn _run(&mut self) -> Result<(), Exception> {
        while !self.call_stack.stack.is_empty() {
            self._interpret()?;
        }

        Ok(())
    }

    /// Interprets the next action based on the current instruction pointer.
    fn _interpret(&mut self) -> Result<(), Exception> {
        let native_ret = match &self.ip {
            Ip::User(ip) => {
                let ip = self._interpret_instruction(*ip)?;
                self.ip = Ip::User(ip);

                None
            },
            Ip::Native(native_call) => {
                let args = NativeCallArgs {};

                let mut native_call = native_call.borrow_mut();
                let ret = native_call.execute(args)?;

                Some(ret)
            },
            Ip::None => {
                // The instruction pointer should only be `None` before the main function is called,
                // i.e. before any stack frame has been entered. Otherwise, there's no reasonable thing to do.
                assert!(
                    self.call_stack.stack.is_empty(),
                    "Instruction pointer is `None`, but the call stack isn't empty. The vm has nowhere to go."
                );

                None
            },
        };

        match native_ret {
            Some(NativeCallControlFlow::Call(closure)) => {
                self._call_closure(closure, 0)?;
            },
            Some(NativeCallControlFlow::Return(value)) => {
                self._return_from_native(value);
            },
            None => {},
        }

        Ok(())
    }

    /// Interprets the current instruction pointed to by `ip`.
    /// Returns the new value the instruction pointer should progress to.
    fn _interpret_instruction(&mut self, mut ip: usize) -> Result<usize, Exception> {
        let opcode = self.consts.code.get(ip)
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

        ip += 1;

        Ok(ip)
    }
}
