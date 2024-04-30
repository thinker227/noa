use crate::runtime::value::{FromValue, Value};
use crate::runtime::opcode::{FuncId, Opcode};
use crate::runtime::frame::StackFrame;
use crate::runtime::exception::{Exception, ExceptionKind};
use super::VM;
use crate::current_frame;

impl VM {
    /// Executes the main function.
    pub fn execute_main(&mut self) -> Result<Value, Exception> {
        let frame = StackFrame::new(self.main);
        self.call_stack.push(frame);

        self.execute()?;

        let exit_value = self.pop()?;
        Ok(exit_value)
    }

    fn execute(&mut self) -> Result<(), Exception> {
        while !self.call_stack.is_empty() {
            self.step()?;
        }

        Ok(())
    }

    fn step(&mut self) -> Result<(), Exception> {
        let frame = current_frame!(self)?;

        let ip = frame.increment_ip();
        let function = self.functions.get(&frame.function())
            .ok_or_else(|| Exception::new(ExceptionKind::InvalidFunction))?;

        let opcode = *function.code().get(ip)
            .ok_or_else(|| Exception::new(ExceptionKind::FunctionOverrun))?;

        self.interpret(opcode)
    }

    fn interpret(&mut self, opcode: Opcode) -> Result<(), Exception> {
        match opcode {
            Opcode::NoOp => {},

            Opcode::Jump(address) => {
                let frame = current_frame!(self)?;
                frame.set_ip(address as usize)
            },
            Opcode::JumpIf(address) => {
                let x = self.pop_as()?;
                if x {
                    let frame = current_frame!(self)?;
                    frame.set_ip(address as usize);
                }
            },
            Opcode::Call(arg_count) => {
                if arg_count > 0 {
                    return Err(Exception::new(ExceptionKind::Unsupported));
                }

                let function = self.pop_as::<FuncId>()?;

                let frame = StackFrame::new(function);

                if self.call_stack.len() >= self.call_stack.capacity() {
                    return Err(Exception::new(ExceptionKind::CallStackOverflow));
                }

                self.call_stack.push(frame);
            },
            Opcode::Ret => {
                let ret_value = self.pop()?;

                // It is impossible to be at this point
                // and for the call stack to be empty at the same time.
                self.call_stack.pop()
                    .expect("call stack should not be empty");

                self.push(ret_value)?;
            },

            Opcode::PushInt(x) => self.push(Value::Number(x))?,
            Opcode::PushBool(x) => self.push(Value::Bool(x))?,
            Opcode::PushFunc(id) => self.push(Value::Function(id))?,
            Opcode::PushNil => self.push(Value::Nil)?,

            Opcode::Pop => {
                self.pop()?;
            },
            Opcode::Dup => {
                let value = self.pop()?;
                self.push(value)?;
                self.push(value)?;
            },

            Opcode::StoreVar(_) => return Err(Exception::new(ExceptionKind::Unsupported)),
            Opcode::LoadVar(_) => return Err(Exception::new(ExceptionKind::Unsupported)),

            Opcode::Add =>  self.binary_op(|a: i32, b: i32| a + b)?,
            Opcode::Sub =>  self.binary_op(|a: i32, b: i32| a - b)?,
            Opcode::Mult => self.binary_op(|a: i32, b: i32| a * b)?,
            Opcode::Div =>  self.binary_op(|a: i32, b: i32| a / b)?,
            Opcode::Equal => {
                let b = self.pop()?;
                let a = self.pop()?;
                let x = (a == b).to_value();

                self.push(x)?;
            },
            Opcode::LessThan => self.binary_op(|a: i32, b: i32| a < b)?,
            Opcode::Not =>      self.unary_op(|x: bool| !x)?,
            Opcode::And =>      self.binary_op(|a: bool, b: bool| a && b)?,
            Opcode::Or =>       self.binary_op(|a: bool, b: bool| a || b)?,
        }

        Ok(())
    }
}
