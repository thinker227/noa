//! # The VM interpreter
//! 
//! The Noa virtual machine is a bytecode interpreter.
//! During normal execution of a user function, the vm will sequentially read bytes
//! from the code section of the Ark file and interpret each byte as an instruction
//! (and potentially read an additional set of bytes as instruction operands).
//! Executing an instruction will usually have some effect on the stack, the instruction pointer,
//! or control flow, causing the vm to potentially enter and execute a new function.
//! 
//! The principal method used to interact with the interpreter is [`Vm::call_run`]
//! which calls a [`Closure`] with some specified arguments and immediately begins executing the interpreter.
//! It acts as a higher-level wrapper around [`Vm::call`] which is the core function used to handle calling functions,
//! which itself delegates to two other functions, [`Vm::call_user`] and [`Vm::call_native`]
//! depending on whether the provided function is a user or native function.
//! 
//! ## The execution root
//! 
//! When calling a function from outside the context of the vm (e.g. from main.rs),
//! the context is called the "execution root". This isn't as much a stack frame as it is
//! a way to refer to the call stack being empty, which is only the case when a function is called like this.
//! Returning from a user function called from the execution root acts much like returning back into
//! a native function, since that is essentially what it does.
//! 
//! ## Calling user functions
//! 
//! A user function is a function defined as a sequence of bytecode instructions.
//! It is the main kind of function the interpreter executes, and is what most of it is dedicated to.
//! 
//! When [`Vm::call_user`] is called, the vm will begin setting itself up for executing the provided closure.
//! This involves pushing the closure captures object onto the stack, preparing the stack with placeholder values
//! for the locals of the function, setting the instruction pointer, and pushing a new stack frame onto the call stack.
//! 
//! Unless there's a bug in the vm, the stack will roughly look like this when calling [`Vm::call_user`]:
//! 
//! ```
//! [ ..., closure, arg1, arg2, arg3 ]
//! ```
//! 
//! The closure will be the first thing pushed onto the stack, followed by the arguments in sequential order.
//! 
//! Within the metadata of a function, the *arity* of the function is specified,
//! i.e. how many arguments the function expects,
//! and the vm needs to respect this in order to not put the stack in an bad state.
//! The way this is done is by pushing [`Value::Nil`] onto the stack until there are enough arguments,
//! or popping values until there are enough.
//! 
//! After the stack has been set up, the vm calculates the stack start index of the function,
//! which is the index within the stack which represents the start of function's own little slice of the stack
//! where its arguments, captures object, and locals will live.
//! 
//! After calling [`Vm::call_user`], the stack will look roughly like this:
//! 
//! ```
//! [ ..., closure, arg1, arg2, arg3, captures, local1, local2, local3 ]
//! //             ^
//! //     stack start index
//! ```
//! 
//! Note that the closure is still on the stack right before the stack start index.
//! 
//! After this, the vm will construct a new stack frame for the user function,
//! which contains the function's ID, the aforementioned stack start index,
//! the address to which the vm should return to after the user function is done executing,
//! and the [`FrameKind`] of the stack frame (which is always [`FrameKind::UserFunction`]),
//! and push it onto the call stack.
//! 
//! Once everything is set up, the [`Vm::ip`] will be set to the function's specified start address,
//! and the [`Vm::call_user`] is finished.
//! 
//! ## Returning from user functions
//! 
//! Returning from a user function is done from bytecode by interpreting the [`opcode::RET`] instruction.
//! This will return from [`Vm::interpret_instruction`] with [`InterpretControlFlow::Return`] and in turn
//! call [`Vm::ret_user`].
//! 
//! [`Vm::ret_user`] in many ways mirrors [`Vm::call_user`] in that it essentially does the reverse of everything
//! [`Vm::call_user`] set up. [`Vm::ret_user`] begins by popping a value off of the stack to use as the return
//! value of the user function. It then pops the current stack frame off of the call stack,
//! ensuring that it is a user function frame, and retrieving the stack backtrack index from the stack frame.
//! This is done through a neat little hack which relies on the layout of the stack when returning from a user function:
//! 
//! ```
//! // When the function was called from a user function:
//! [ ..., closure, arg1, arg2, arg3, captures, local1, local2, local3, ... ]
//! //             ^
//! //     stack start index
//! 
//! // When the function was called from a native function or the execution root:
//! [ ..., arg1, arg2, arg3, captures, local1, local2, local3, ... ]
//! //    ^
//! // stack start index
//! ```
//! 
//! We want to get rid of everything on the stack related to the user function, including the closure itself.
//! However, there will only be a closure on the stack if the function was itself called from a user function.
//! The logic here is therefore that the index on the stack to backtrack to when returning is the
//! index before ths stack start index *if called from a user function*, otherwise it is just the stack start index.
//! 
//! After the stack has been reset, the [`Vm::ip`] is set to the [`Frame::ret`] of the previous stack frame,
//! if it's not [`None`]. In the case that it is [`None`], the function must've been called from the
//! execution root or a native function, so there's no need to set the instruction pointer to anything.
//! 
//! ## Calling native functions
//! 
//! Calling native function is in many ways similar to calling a user function, but different in some crucial ways
//! due to having to call into native code. Native functions naturally do not execute any bytecode,
//! instead they have to interrupt normal execution of bytecode and instead execute a native Rust function.
//! 
//! A problem, however, arises when considering that native functions may want to call back into user code,
//! because this requires pausing execution of the native function and returning into user code.
//! I initially experimented with representing native functions as state machines to allow simply pausing
//! them, but it turned out to be way too complex. Instead, native functions calling user code is implemented
//! using simple recursion. A native function can call [`Vm::call_run`] and provide its own closure and arguments,
//! which will execute whatever function was specified by interpreting bytecode.
//! 
//! [`Vm::call_native`] is fundamentally different from [`Vm::call_user`] in that [`Vm::call_native`] will return
//! the return value of its called function, while [`Vm::call_user`] returns by pushing a value onto the stack.
//! [`Vm::call_native`] will also push a stack frame onto the call stack only for the duration of the execution
//! of the native function and immediately pop it afterwards. It also doesn't account for any superfluous arguments
//! to the function since native function may be variadic,
//! instead passing a vector of the raw arguments to the native function.
//! 
//! ## Temporary stack frames
//! 
//! Temporary stack frames ([`FrameKind::Temp`]) exist as a consequence of Noa being expression-oriented
//! (and the compiler not being very smart lol). The compiler cannot know how big the stack will be
//! when breaking or continuing from a loop, since the `break` or `continue` statement might
//! be inside some other expression like `1 + break`. In this example, `1` will be on the stack when breaking,
//! essentially being garbage data. To remedy this, when entering a loop, the [`opcode::ENTER_TEMP_FRAME`]
//! opcode will be emitted, which signifies to push a temporary stack frame onto the call stack,
//! and when exiting a loop, the [`opcode::EXIT_TEMP_FRAME`] opcode is emitted which does the reverse.
//! Despite being called the "*call* stack", temporary frames don't really represent a "call" to anything,
//! instead just being a marker on the call stack. For this reason, several places in the interpreter
//! have to account for the current stack frame possibly being a temporary frame.
//! 
//! Entering a temporary stack frame isn't particularly complex, however.
//! Really all that is done is looking up the current *user function* frame to retrieve its index on the call stack,
//! constructing an almost identical copy of that frame though with a different stack start index and kind,
//! and pushing that onto the stack,.
//! 
//! Exiting a temporary stack frame isn't particularly complex either.
//! It just pops the current frame, ensures that it is a temporary frame, and resets the stack back to the frame's
//! stack start index. Note that it *does not* set the instruction pointer, because exiting a temporary stack frame
//! has nothing to do with returning from a function, since they are primarily used for `break`/`continue` within loops.
//! 
//! ## Boundaries
//! 
//! On a small note, [`opcode::BOUNDARY`] exists to catch any missing return statements.
//! [`opcode::BOUNDARY`] is emitted after every function, and its only purpose is to cause an exception
//! if the vm tries to execute it.

use std::assert_matches::assert_matches;
use std::collections::HashMap;

use crate::ark::FuncId;
use crate::exception::Exception;
use crate::heap::{HeapGetError, HeapValue};
use crate::opcode;
use crate::value::{Closure, Field, List, Object, Value};
use crate::vm::frame::{Frame, FrameKind};

use super::debugger::DebugInspection;
use super::{Vm, Result};

enum InterpretControlFlow {
    Continue,
    Call {
        closure: Closure,
        arg_count: u32,
    },
    Return,
}

impl Vm {
    /// Calls a closure with specified arguments, runs until it returns, then returns the return value of the closure.
    pub fn call_run(&mut self, closure: Closure, args: &[Value]) -> Result<Value> {
        // Push arguments onto the stack.
        for value in args {
            self.stack.push(*value)
                .map_err(|e| self.exception(e))?;
        }

        self.call(closure, args.len() as u32)?;
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
        // meaning that, since the arguments and captures are already on the stack in the correct order,
        // we can just set the frame's stack start index to the current stack head
        // minus the function's arity.
        let stack_start = self.stack.head() - arity as usize;

        // Push captures onto the stack as additional "arguments".
        if let Some(heap_address) = closure.captures {
            // Cannot use `self.get_heap_value` here because then the borrow checker thinks
            // we're borrowing self while we actually just intend to borrow `self.heap`.
            let heap_value = self.heap.get(heap_address)
                .map_err(|e| {
                    let ex = match e {
                        HeapGetError::OutOfBounds => Exception::OutOfBoundsHeapAddress,
                        HeapGetError::SlotFreed => Exception::FreedHeapAddress,
                    };
                    self.exception(ex)
                })?;
            
            let list = match heap_value {
                HeapValue::List(list) => list,
                _ => panic!("expected closure captures to be a list")
            };
            
            for value in &list.0 {
                self.stack.push(*value)
                    .map_err(|e| self.exception(e))?;
            }
        };

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

        self.call_stack.push_within_capacity(frame)
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

        let ret_address = self.get_return_address();

        let frame = Frame {
            function: id,
            stack_start,
            ret: ret_address,
            kind: FrameKind::NativeFunction,
        };

        // Block to keep track of the 'scope' for the frame.
        let ret = {
            self.call_stack.push_within_capacity(frame)
                .map_err(|_| self.exception(Exception::CallStackOverflow))?;

            // Actually call the function.
            // The function might call `call_run` and enter recursion within the vm,
            // which is why we need an exclusive reference to the vm.
            let ret = function(self, args)?;

            self.call_stack.pop();

            ret
        };

        let stack_backtrack_index = self.get_stack_backtrack_index(stack_start);
        self.stack.shrink(stack_backtrack_index);

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
        match self.call_stack.last() {
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

        let frame = self.call_stack.pop()
            .expect("call stack cannot be empty when returning from a user function");

        assert_matches!(
            frame.kind,
            FrameKind::UserFunction | FrameKind::Temp { .. },
            "top-most stack frame has to be a user function user temporary frame when returning from a user function"
        );

        let stack_start = frame.stack_start;
        let stack_backtrack_index = self.get_stack_backtrack_index(stack_start);
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

    /// Gets the index on the stack to backtrack to when returning from a function.
    fn get_stack_backtrack_index(&self, stack_start: usize) -> usize {

        // When calling a function from a user function, the stack will approximately look like this:
        // 
        // [ ..., closure, arg1, arg2, arg3, ... ]
        //                   ^
        //       this is the current frame's
        //       (the one we just popped's)
        //           stack start index
        // 
        // This way, if we shrink the stack back to the frame's stack start index - 1,
        // we get rid of the closure since that has been "consumed" by calling the function.
        // However, if the function was called from a native function or the execution root,
        // there won't be a closure there, so we don't want to shrink by the additional index backwards.
        
        match self.get_top_non_temp_frame() {
            Some(Frame { kind: FrameKind::NativeFunction, .. }) | None => stack_start,
            _ => stack_start - 1
        }
    }

    /// Gets the top-most stack frame off of the call stack which is not a temporary frame.
    fn get_top_non_temp_frame(&self) -> Option<&Frame> {
        match self.call_stack.last() {
            Some(Frame { kind: FrameKind::Temp { parent_function_index }, .. }) => Some(
                self.call_stack.get(*parent_function_index)
                    .expect("parent function index of temporary stack frame should point to a valid stack frame")
            ),
            Some(frame) => Some(frame),
            None => None,
        }
    }

    /// Enters a temporary stack frame.
    fn enter_temp_frame(&mut self) -> Result<()> {
        let current_frame = self.call_stack.last()
            .expect("call stack should not be empty while entering temporary stack frame");

        let current_frame_index = match &current_frame.kind {
            FrameKind::UserFunction => self.call_stack.len() - 1,
            FrameKind::NativeFunction => panic!("top-most frame of call stack cannot be a native function frame while enter a temporary stack frame"),
            FrameKind::Temp { parent_function_index } => *parent_function_index,
        };

        let stack_start = self.stack.head();

        let frame = Frame {
            stack_start,
            kind: FrameKind::Temp { parent_function_index: current_frame_index },
            .. *current_frame
        };

        self.call_stack.push_within_capacity(frame)
            .map_err(|_| self.exception(Exception::CallStackOverflow))?;

        Ok(())
    }

    /// Exits a temporary stack frame.
    fn exit_temp_frame(&mut self) -> Result<()> {
        let current_frame = self.call_stack.pop()
            .expect("call stack should not be empty while exiting temporary stack frame");

        assert_matches!(
            current_frame.kind,
            FrameKind::Temp { .. },
            "top-most stack frame while exiting a temporary stack frame should be a temporary stack frame"
        );

        self.stack.shrink(current_frame.stack_start);

        Ok(())
    }

    fn get_variable_stack_index(&self, variable_index: usize) -> usize {
        let frame = self.get_top_non_temp_frame()
            .expect("top-most stack frame should not be empty when reading a variable");

        frame.stack_start + variable_index
    }

    fn read_variable(&self, variable_index: usize) -> Result<Value> {
        let stack_index = self.get_variable_stack_index(variable_index);

        let val = self.stack.get(stack_index)
            .ok_or_else(|| self.exception(Exception::InvalidVariable(variable_index)))?;

        Ok(*val)
    }

    fn write_variable(&mut self, variable_index: usize, value: Value) -> Result<()> {
        let stack_index = self.get_variable_stack_index(variable_index);

        let stack_value = match self.stack.get_mut(stack_index) {
            Some(x) => x,
            None => return Err(self.exception(Exception::InvalidVariable(stack_index))),
        };

        *stack_value = value;

        Ok(())
    }

    /// Runs the interpreter until the current function returns, or an exception occurs.
    fn run_function(&mut self) -> Result<Value> {
        // This feels like such a hack lol
        let mut depth: u32 = 0;

        while !self.call_stack.is_empty() {
            self.trace_ip = self.ip;

            // Todo: only do this when a breakpoint is reached.
            if let Some(debugger) = &mut self.debugger {
                // Break for the debugger and allow it to inspect the VM's state.

                let inspection = DebugInspection {
                    consts: &self.consts,
                    stack: &self.stack,
                    heap: &self.heap,
                    call_stack: &self.call_stack,
                    ip: self.ip
                };

                debugger.debug_break(inspection);
            }

            let ctrl_flw = self.interpret_instruction()?;

            match ctrl_flw {
                InterpretControlFlow::Continue => {},
                InterpretControlFlow::Call { closure, arg_count } => {
                    self.call(closure, arg_count)?;

                    depth += 1;
                },
                InterpretControlFlow::Return => {
                    let ret = self.ret_user()?;
                    
                    if depth == 0 {
                        return Ok(ret);
                    }

                    self.push(ret)?;
                    
                    depth -= 1;
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

    /// Reads a [`f64`] and progresses the instruction pointer by 8.
    fn read_f64(&mut self) -> Result<f64> {
        let bytes: [u8; 8] = self.consts.code.get(self.ip..(self.ip + 8))
            .ok_or_else(|| self.exception(Exception::Overrun))?
            .try_into()
            .unwrap(); // safe because if `get` returns `Some`,
                       // the slice will always be 8 elements long
        
        self.ip += 8;
        
        let val = f64::from_be_bytes(bytes);
        Ok(val)
    }

    /// Pops a value off the stack.
    fn pop(&mut self) -> Result<Value> {
        self.stack.pop()
            .map_err(|_| self.exception(Exception::StackUnderflow))
    }

    /// Pops a value off the stack and performs a coercion on it into a specified type.
    fn pop_val_as<'a, T>(
        &'a mut self,
        coerce: impl FnOnce(&'a Self, Value) -> Result<T>
    ) -> Result<T> {
        let val = self.stack.pop()
            .map_err(|_| self.exception(Exception::StackUnderflow))?;

        coerce(self, val)
    }

    fn pop_val_as_mut<'a, T>(
        &'a mut self,
        coerce: impl FnOnce(&'a mut Self, Value) -> Result<T>
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

        self.ip += 1;

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
                // [ ..., closure, arg1, arg2, arg3, ... ]
                //                                  ^
                //                          current stack head
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

            opcode::PUSH_FLOAT => {
                let val = self.read_f64()?;

                self.push(Value::Number(val))?;
            },

            opcode::PUSH_BOOL => {
                let bool = self.read_u8()? != 0;

                self.push(Value::Bool(bool))?;
            },

            opcode::PUSH_FUNC => {
                let index = self.read_u32()?;
                
                let function = self.consts.functions.get(index as usize)
                    .ok_or_else(|| self.exception(Exception::InvalidUserFunction(index)))?;
                
                // Save captured variables as a list.
                let captures = if !function.captures.is_empty() {
                    let mut captures = Vec::with_capacity(function.captures.len());
                    for capture_index in &function.captures {
                        let val = self.read_variable(*capture_index as usize)?;
                        captures.push(val);
                    }

                    let address = self.heap_alloc(HeapValue::List(List(captures)))?;

                    Some(address)
                } else {
                    None
                };

                let closure = Closure {
                    function: FuncId(index),
                    captures
                };

                self.push(Value::Function(closure))?;
            },

            opcode::PUSH_NIL => {
                self.push(Value::Nil)?;
            },

            opcode::PUSH_STRING => {
                let index = self.read_u32()? as usize;
                
                self.push(Value::InternedString(index))?;
            },

            opcode::PUSH_OBJECT => {
                let dynamic = self.read_u8()? != 0;

                let object = Object {
                    fields: HashMap::new(),
                    dynamic
                };
                let adr = self.heap_alloc(HeapValue::Object(object))?;

                self.push(Value::Object(adr))?;
            },

            opcode::PUSH_LIST => {
                let list = List(Vec::new());
                let adr = self.heap_alloc(HeapValue::List(list))?;

                self.push(Value::Object(adr))?;
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

            opcode::STORE_VAR => {
                let var_index = self.read_u32()?;

                let value = self.pop()?;

                self.write_variable(var_index as usize, value)?;
            },

            opcode::LOAD_VAR => {
                let var_index = self.read_u32()?;

                let value = self.read_variable(var_index as usize)?;

                self.push(value)?;
            },

            opcode::STORE_VAR_BOXED => {
                todo!()
            },

            opcode::ADD => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| a + b
                )?;
            },

            opcode::SUB => {
                self.binary_op(
                    Self::coerce_to_number,
                    |a, b| b - a
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

            opcode::EQUAL => {
                let a = self.pop()?;
                let b = self.pop()?;

                let val = self.equal(a, b)?;

                self.push(Value::Bool(val))?;
            },

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

            opcode::CONCAT => {
                let other = self.pop_val_as(Self::to_string)?;
                let mut str = self.pop_val_as(Self::to_string)?;

                str.push_str(other.as_str());

                // Todo: garbage collection is never actually run, so this leaks memory currently.
                let adr = self.heap_alloc(HeapValue::String(str))?;

                self.push(Value::Object(adr))?;
            },

            opcode::TO_STRING => {
                let val = self.pop()?;

                let str = self.to_string(val)?;

                // Todo: garbage collection is never actually run, so this leaks memory currently.
                let adr = self.heap_alloc(HeapValue::String(str))?;

                self.push(Value::Object(adr))?;
            },

            opcode::ADD_FIELD => {
                let mutable = self.read_u8()? != 0;

                let val = self.pop()?;
                let name = self.pop_val_as(Self::to_string)?;
                let (obj, _) = self.pop_val_as_mut(Self::coerce_to_object_mut)?;

                let field_count = obj.fields.len() as u32;
                obj.fields.insert(name, Field {
                    val,
                    mutable,
                    index: field_count
                });
            },

            opcode::WRITE_FIELD => {
                let val = self.pop()?;
                let name = self.pop_val_as(Self::to_string)?;
                let (obj, _) = self.pop_val_as_mut(Self::coerce_to_object_mut)?;

                let dynamic = obj.dynamic;
                
                match obj.fields.get_mut(&name) {
                    Some(field) => {
                        if field.mutable {
                            // Override value.
                            field.val = val;
                        } else {
                            // Cannot write to immutable field.
                            return Err(self.exception(Exception::WriteToImmutableField(name)));
                        }
                    },
                    None => {
                        if dynamic {
                            // Writing to a dynamic object, insert a mutable field.
                            let field_count = obj.fields.len() as u32;
                            obj.fields.insert(name, Field {
                                val,
                                mutable: true,
                                index: field_count
                            });
                        } else {
                            // Missing field.
                            return Err(self.exception(Exception::MissingField(name)));
                        }
                    }
                };
            },

            opcode::READ_FIELD => {
                let name = self.pop_val_as(Self::to_string)?;
                let (obj, _) = self.pop_val_as(Self::coerce_to_object)?;

                match obj.fields.get(&name).copied() {
                    Some(field) => {
                        self.push(field.val)?;
                    },
                    None => {
                        return Err(self.exception(Exception::MissingField(name)));
                    },
                };
            },

            opcode::APPEND_ELEMENT => {
                let value = self.pop()?;
                let (list, _) = self.pop_val_as_mut(Self::coerce_to_list_mut)?;

                list.0.push(value);
            },

            opcode::WRITE_ELEMENT => {
                let value = self.pop()?;

                let raw_index = self.pop_val_as(Self::coerce_to_number)?;
                let index = self.to_integer(raw_index)?;

                let (list, _) = self.pop_val_as_mut(Self::coerce_to_list_mut)?;

                let length = list.0.len();

                if index < 0 {
                    return Err(self.exception(Exception::OutOfBoundsIndex(raw_index, length)));
                }

                let index = index as usize;

                if index >= length {
                    return Err(self.exception(Exception::OutOfBoundsIndex(raw_index, length)));
                }

                let element = list.0.get_mut(index).expect("index should be valid");
                *element = value;
            },

            opcode::READ_ELEMENT => {
                let raw_index = self.pop_val_as(Self::coerce_to_number)?;
                let index = self.to_integer(raw_index)?;

                let (list, _) = self.pop_val_as(Self::coerce_to_list)?;

                let length = list.0.len();

                if index < 0 {
                    return Err(self.exception(Exception::OutOfBoundsIndex(raw_index, length)));
                }

                let index = index as usize;

                if index >= length {
                    return Err(self.exception(Exception::OutOfBoundsIndex(raw_index, length)));
                }

                let element = *list.0.get(index).expect("index should be valid");
                self.push(element)?;
            },

            opcode::BOX => {
                let val = self.pop()?;
                // Important that we unbox the value so we don't get a boxed box.
                let unboxed = self.unbox(val)?;

                let boxed = self.heap_alloc(HeapValue::Box(unboxed))?;
                self.push(Value::Object(boxed))?;
            },

            opcode::UNBOX => {
                let val = self.pop()?;
                let unboxed = self.unbox(val)?;
                self.push(unboxed)?;
            },

            opcode::BOUNDARY => return Err(self.exception(Exception::Overrun)),

            _ => return Err(self.exception(Exception::UnknownOpcode(*opcode)))
        }
        
        Ok(InterpretControlFlow::Continue)
    }
}
