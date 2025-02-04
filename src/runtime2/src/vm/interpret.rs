use crate::ark::FuncId;
use crate::exception::Exception;
use crate::opcode;
use crate::value::{Closure, Value};
use crate::vm::frame::{Frame, FrameKind};

use super::{Vm, Result};

enum InterpretControlFlow {
    Continue,
    Call {
        closure: Closure,
        arg_count: u32,
    },
    Return,
}

impl Vm<'_> {
    /// Calls a closure with specified arguments, runs until it returns, then returns the return value of the closure.
    pub fn call_run(&mut self, function: FuncId, args: &[Value]) -> Result<Value> {
        // Push arguments onto the stack.
        for value in args {
            self.stack.push(*value)
                .map_err(|e| self.exception(e))?;
        }

        self.call(function.into(), args.len() as u32)?;
        let res = self.run_function()?;

        Ok(res)
    }

    /// Calls a closure with a specified amount of arguments from the stack.
    fn call(&mut self, closure: Closure, arg_count: u32) -> Result<()> {
        if closure.function.is_native() {
            // It shouldn't be possible in any way for a native function to capture variables.
            assert!(closure.captures.is_none(), "Native function cannot be called with captures.");
            self.call_native(closure.function, arg_count)
        } else {
            self.call_user(closure, arg_count)
        }
    }

    /// Calls a user function as a closure.
    fn call_user(&mut self, closure: Closure, arg_count: u32) -> Result<()> {
        // todo: figure out how to support captured variables
        // they should probably just be some variety of function arguments
        if closure.captures.is_some() {
            todo!("figure out how to support captured variables")
        }

        let user_index = closure.function.decode();
        let function = self.consts.functions.get(user_index as usize)
            .ok_or_else(|| self.exception(Exception::InvalidUserFunction(user_index)))?;

        let arity = function.arity;
        let locals_count = function.locals_count;
        let address = function.address as usize;
        let ret = self.get_return_address();

        // Get rid of any additional arguments outside of what the function expects.
        if arg_count > arity {
            for _ in arity..arg_count {
                self.stack.pop()
                    .map_err(|e| self.exception(e))?;
            }
        }

        // Fill out the stack with missing arguments if there are not enough.
        if arg_count < arity {
            for _ in arg_count..arity {
                self.stack.push(Value::Nil)
                    .map_err(|e| self.exception(e))?;
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
            self.stack.push(Value::Nil)
                .map_err(|e| self.exception(e))?;
        }

        let frame = Frame {
            function: function.id,
            stack_start,
            ret,
            kind: FrameKind::UserFunction,
        };

        self.call_stack.stack.push_within_capacity(frame)
            .map_err(|_| self.exception(Exception::CallStackOverflow))?;
        
        self.ip = address;

        Ok(())
    }

    /// Calls a native function.
    fn call_native(&mut self, id: FuncId, arg_count: u32) -> Result<()> {
        let native_index = id.decode();
        let _function = self.consts.native_functions.get(native_index as usize)
            .ok_or_else(|| self.exception(Exception::InvalidNativeFunction(native_index)))?;

        let stack_start = self.stack.head() - arg_count as usize;

        let _args = self.stack.slice_from_end(arg_count as usize)
            .ok_or(self.exception(Exception::StackUnderflow))?;

        let ret = self.get_return_address();

        let frame = Frame {
            function: id,
            stack_start,
            ret,
            kind: FrameKind::NativeFunction,
        };

        self.call_stack.stack.push_within_capacity(frame)
            .map_err(|_| self.exception(Exception::CallStackOverflow))?;

        todo!()
    }

    /// Gets the return address for a function invocation during the current state of the vm.
    fn get_return_address(&self) -> Option<usize> {
        // The return address for a new function call is the current instruction pointer
        // if the current stack frame is a user function frame or a temporary frame.
        // For native functions, having a specific return address wouldn't make a lot of sense.
        // For the case that the call stack is empty, the caller must be the execution root.
        match self.call_stack.stack.last() {
            Some(frame) => match frame.kind {
                FrameKind::UserFunction | FrameKind::Temp { .. } => Some(self.ip),
                FrameKind::NativeFunction => None,
            },
            None => None,
        }
    }

    /// Returns from the current user function and returns the current top-most value on the stack.
    fn ret_user(&mut self) -> Result<Value> {
        todo!()
    }

    /// Runs the interpreter until the call stack runs out, or an exception occurs.
    fn _run(&mut self) -> Result<()> {
        while !self.call_stack.stack.is_empty() {
            let ctrl_flw = self.interpret_instruction()?;

            match ctrl_flw {
                InterpretControlFlow::Continue => {},
                InterpretControlFlow::Call { closure, arg_count } => {
                    self.call(closure, arg_count)?;
                },
                InterpretControlFlow::Return => {
                    let ret = self.ret_user()?;
                    self.stack.push(ret)
                        .map_err(|e| self.exception(e))?;
                },
            }
        }

        Ok(())
    }

    /// Runs the interpreter until the current function returns, or an exception occurs.
    fn run_function(&mut self) -> Result<Value> {
        while !self.call_stack.stack.is_empty() {
            let ctrl_flw = self.interpret_instruction()?;

            match ctrl_flw {
                InterpretControlFlow::Continue => {},
                InterpretControlFlow::Call { closure, arg_count } => {
                    self.call(closure, arg_count)?;
                },
                InterpretControlFlow::Return => {
                    let ret = self.ret_user()?;
                    return Ok(ret);
                },
            }
        }

        // If we got here then the call stack somehow ran out without the function returning.
        Err(self.exception(Exception::NoReturn))
    }

    /// Interprets the current instruction pointed to by `ip`.
    /// Returns the new value the instruction pointer should progress to.
    fn interpret_instruction(&mut self) -> Result<InterpretControlFlow> {
        let opcode = self.consts.code.get(self.ip)
            .ok_or(self.exception(Exception::Overrun))?;

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

            opcode::BOUNDARY => return Err(self.exception(Exception::Overrun)),

            _ => return Err(self.exception(Exception::UnknownOpcode(*opcode)))
        }
        
        Ok(InterpretControlFlow::Continue)
    }
}
