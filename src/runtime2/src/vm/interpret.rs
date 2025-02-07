use std::assert_matches::assert_matches;

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

        // Get the function from the decoded function ID.
        let user_index = closure.function.decode();
        let function = self.consts.functions.get(user_index as usize)
            .ok_or_else(|| self.exception(Exception::InvalidUserFunction(user_index)))?;

        // Set up properties for the call and stack frame.
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
        // Get the function from the decoded function ID.
        // Function pointers implement `Copy`, so retrieving the function pointer here
        // doesn't actually require an immutable borrow, which is incredibly nice.
        let native_index = id.decode();
        let function = *self.consts.native_functions.get(native_index as usize)
            .ok_or_else(|| self.exception(Exception::InvalidNativeFunction(native_index)))?;

        let stack_start = self.stack.head() - arg_count as usize;

        let args = self.stack.slice_from_end(arg_count as usize)
            .ok_or_else(|| self.exception(Exception::StackUnderflow))?
            .to_vec();

        let ret = self.get_return_address();

        let frame = Frame {
            function: id,
            stack_start,
            ret,
            kind: FrameKind::NativeFunction,
        };

        // Block to keep track of the 'scope' for the frame.
        let ret = {
            self.call_stack.stack.push_within_capacity(frame)
                .map_err(|_| self.exception(Exception::CallStackOverflow))?;

            // Actually call the function.
            // The function might call `call_run` and enter recursion within the vm,
            // which is why we need an exclusive reference to the vm.
            let ret = function(self, args)
                .map_err(|e| self.exception(e))?;

            self.call_stack.stack.pop();

            ret
        };

        // If nothing has gone wrong then this shouldn't even be needed
        // since native functions shouldn't be able to push stuff onto the stack by themselves,
        // but doing this just in case.
        self.stack.shrink(stack_start);

        // Finally, push the return value onto the stack.
        self.stack.push(ret)
            .map_err(|e| self.exception(e))?;

        Ok(())
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
        // The return value will be at the very top of the stack when returning.
        let ret = self.stack.pop()
            .map_err(|e| self.exception(e))?;

        let frame = self.call_stack.stack.pop()
            .expect("call stack cannot be empty when returning from a user function");

        assert_matches!(
            frame.kind,
            FrameKind::UserFunction | FrameKind::Temp { .. },
            "top-most stack frame has to be a user function user temporary frame when returning from a user function"
        );

        let stack_start = frame.stack_start;

        // When calling a function from a user function, the stack will approximately look like this:
        // 
        // [ ..., <closure>, arg1, arg2, arg3, ... ]
        //                   ^
        //       this is the current frame's
        //       (the one we just popped's)
        //           stack start index
        // 
        // This way, if we shrink the stack back to the frame's stack start index - 1,
        // we get rid of the closure since that has been "consumed" by calling the function.
        // However, if the function was called from a native function or the execution root,
        // there won't be a closure there, so we don't want to shrink by the additional index backwards.
        
        let stack_backtrack_index = match self.get_top_non_temp_frame() {
            Some(Frame { kind: FrameKind::NativeFunction, .. }) | None => stack_start,
            _ => stack_start - 1
        };

        self.stack.shrink(stack_backtrack_index);

        // If the frame has no assigned return address
        // then we're returning to a native function or the execution root.
        // Not setting the instruction pointer in this case is slightly dangerous in case this assumption is wrong
        // or something tries to read the instruction pointer before it's been set back to a meaningful value,
        // which will most likely be an extremely annoying bug to track down, but it's more annoying
        // to have the instruction pointer be an `Option<usize>` or something, so this will have to do.

        if let Some(ret_ip) = frame.ret {
            self.ip = ret_ip;
        }

        Ok(ret)
    }

    /// Gets the top-most stack frame off of the call stack which is not a temporary frame.
    fn get_top_non_temp_frame(&self) -> Option<&Frame> {
        match self.call_stack.stack.last() {
            Some(Frame { kind: FrameKind::Temp { parent_function_index }, .. }) => Some(
                self.call_stack.stack.get(*parent_function_index)
                    .expect("parent function index of temporary stack frame should point to a valid stack frame")
            ),
            Some(frame) => Some(frame),
            None => None,
        }
    }

    /// Enters a temporary stack frame.
    fn enter_temp_frame(&mut self) -> Result<()> {
        let current_frame = self.call_stack.stack.last()
            .expect("call stack should not be empty while entering temporary stack frame");

        let current_frame_index = match &current_frame.kind {
            FrameKind::UserFunction => self.call_stack.stack.len() - 1,
            FrameKind::NativeFunction => panic!("top-most frame of call stack cannot be a native function frame while enter a temporary stack frame"),
            FrameKind::Temp { parent_function_index } => *parent_function_index,
        };

        let stack_start = self.stack.head();

        let frame = Frame {
            stack_start,
            kind: FrameKind::Temp { parent_function_index: current_frame_index },
            .. *current_frame
        };

        self.call_stack.stack.push_within_capacity(frame)
            .map_err(|_| self.exception(Exception::CallStackOverflow))?;

        Ok(())
    }

    /// Exits a temporary stack frame.
    fn exit_temp_frame(&mut self) -> Result<()> {
        let current_frame = self.call_stack.stack.pop()
            .expect("call stack should not be empty while exiting temporary stack frame");

        assert_matches!(
            current_frame.kind,
            FrameKind::Temp { .. },
            "top-most stack frame while exiting a temporary stack frame should be a temporary stack frame"
        );

        self.stack.shrink(current_frame.stack_start);

        Ok(())
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

    /// Reads a [`u8`] and progresses the instruction pointer by 1.
    fn read_u8(&mut self) -> Result<u8> {
        let byte = self.consts.code.get(self.ip)
            .ok_or_else(|| self.exception(Exception::Overrun))?;

        self.ip += 1;

        Ok(*byte)
    }

    /// Reads a [`u32`] and progresses the instruction pointer by 4.
    fn read_u32(&mut self) -> Result<u32> {
        let bytes: [u8; 4] = self.consts.code.get(self.ip..(self.ip + 4))
            .ok_or_else(|| self.exception(Exception::Overrun))?
            .try_into()
            .unwrap(); // safe because if `get` returns `Some`,
                       // the slice will always be 4 elements long
        
        self.ip += 4;
        
        let val = u32::from_be_bytes(bytes);
        Ok(val)
    }

    /// Pops a value off the stack.
    fn pop(&mut self) -> Result<Value> {
        self.stack.pop()
            .map_err(|_| self.exception(Exception::StackUnderflow))
    }

    /// Pops a value off the stack and performs a coercion on it into a specified type.
    fn pop_val_as<T>(
        &mut self,
        coerce: impl FnOnce(&Self, Value) -> Result<T>
    ) -> Result<T> {
        let val = self.stack.pop()
            .map_err(|_| self.exception(Exception::StackUnderflow))?;

        coerce(self, val)
    }

    /// Pushes a value onto the stack.
    fn push(&mut self, val: Value) -> Result<()> {
        self.stack.push(val)
            .map_err(|_| self.exception(Exception::StackOverflow))
    }

    /// Pushes a value off the stack, performs a coercion on it into a specified type,
    /// performs a unary operation on it, then turns it back into a value
    /// and pushes it back onto the stack.
    fn unary_op<T, U: Into<Value>>(
        &mut self,
        coerce: impl FnOnce(&Self, Value) -> Result<T>,
        op: impl FnOnce(T) -> U
    ) -> Result<()> {
        let x = self.pop_val_as(coerce)?;

        let val = op(x);

        self.push(val.into())?;

        Ok(())
    }

    /// Pushes two values off the stack, performs coercions on both values into a specified type,
    /// performs a binary operation on them, then turns the result back into a value
    /// and pushes it back onto the stack.
    fn binary_op<T, U: Into<Value>>(
        &mut self,
        coerce: impl Fn(&Self, Value) -> Result<T>,
        op: impl FnOnce(T, T) -> U
    ) -> Result<()> {
        let a = self.pop_val_as(&coerce)?;
        let b = self.pop_val_as(&coerce)?;

        let val = op(a, b);

        self.push(val.into())?;

        Ok(())
    }

    /// Interprets the current instruction pointed to by `ip`.
    /// Returns the new value the instruction pointer should progress to.
    fn interpret_instruction(&mut self) -> Result<InterpretControlFlow> {
        let opcode = self.consts.code.get(self.ip)
            .ok_or_else(|| self.exception(Exception::Overrun))?;

        match *opcode {
            opcode::NO_OP => {},

            opcode::JUMP => {
                let address = self.read_u32()? as usize;
                self.ip = address;
            },

            opcode::JUMP_IF => {
                let address = self.read_u32()? as usize;

                let val = self.pop_val_as(Self::coerce_to_bool)?;

                if val {
                    self.ip = address;
                }
            },

            opcode::CALL => {
                let arg_count = self.read_u32()?;

                // When calling a function from a user function, the stack will approximately look like this:
                // 
                // [ ..., <closure>, arg1, arg2, arg3, ... ]
                //                                      ^
                //                              current stack head
                //
                // The index on the stack where the closure to call is located at will therefore be
                // the current stack head - the amount of arguments - 1.

                let function_stack_index = self.stack.head() - arg_count as usize - 1;

                let val = self.stack.get(function_stack_index)
                    .ok_or_else(|| self.exception(Exception::StackUnderflow))?;
                let closure = self.coerce_to_function(*val)?;

                return Ok(InterpretControlFlow::Call { closure, arg_count });
            },

            opcode::RET => {
                return Ok(InterpretControlFlow::Return);
            },

            opcode::ENTER_TEMP_FRAME => {
                self.enter_temp_frame()?;
            },

            opcode::EXIT_TEMP_FRAME => {
                self.exit_temp_frame()?;
            },

            opcode::PUSH_INT => {
                let val = self.read_u32()?;

                self.push(Value::Number(val as f64))?;
            },

            opcode::PUSH_BOOL => {
                let val = self.read_u8()?;

                let bool = val != 0;

                self.push(Value::Bool(bool))?;
            },

            opcode::PUSH_FUNC => {
                let val = self.read_u32()?;

                let closure = Closure {
                    function: FuncId(val),
                    captures: None
                };

                self.push(Value::Function(closure))?;
            },

            opcode::PUSH_NIL => {
                self.push(Value::Nil)?;
            },

            opcode::POP => {
                self.pop()?;
            },

            opcode::DUP => {
                let val = self.pop()?;

                self.push(val)?;
                self.push(val)?;
            },

            opcode::SWAP => {
                let a = self.pop()?;
                let b = self.pop()?;

                self.push(a)?;
                self.push(b)?;
            },

            opcode::STORE_VAR => todo!(),

            opcode::LOAD_VAR => todo!(),

            opcode::ADD => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| a + b
                )?;
            },

            opcode::SUB => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| a - b
                )?;
            },

            opcode::MULT => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| a * b
                )?;
            },

            opcode::DIV => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| b / a
                )?;
            },

            opcode::EQUAL => todo!(),

            opcode::LESS_THAN => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| b < a
                )?;
            },

            opcode::NOT => {
                self.unary_op(
                    Self::coerce_to_bool,
                    |x| !x
                )?;
            },

            opcode::AND => {
                self.binary_op(
                    Self::coerce_to_bool,
                    |a, b| a && b
                )?;
            },

            opcode::OR => {
                self.binary_op(
                    Self::coerce_to_bool,
                    |a, b| a || b
                )?;
            },

            opcode::GREATER_THAN => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| b > a
                )?;
            },

            opcode::BOUNDARY => return Err(self.exception(Exception::Overrun)),

            _ => return Err(self.exception(Exception::UnknownOpcode(*opcode)))
        }
        
        Ok(InterpretControlFlow::Continue)
    }
}
