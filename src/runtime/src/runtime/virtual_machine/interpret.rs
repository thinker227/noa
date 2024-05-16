use crate::runtime::value::{FromValue, Value};
use crate::runtime::opcode::{FuncId, Opcode};
use crate::runtime::exception::{CodeException, Exception, ExceptionData};
use super::VM;
use crate::current_frame_mut;

impl VM {
    /// Executes the main function.
    pub fn execute_main(&mut self) -> Result<Value, Exception> {
        self.call(self.main, 0, true)?;

        self.execute()?;

        let exit_value = self.pop()?;
        Ok(exit_value)
    }

    fn execute(&mut self) -> Result<(), Exception> {
        while !self.call_stack.is_empty() {
            let res = self.step();

            if let Err(data) = res {
                let stack_trace = self.get_stack_trace();
                let ex = Exception::new(data, stack_trace);
                return Err(ex);
            }
        }

        Ok(())
    }

    fn step(&mut self) -> Result<(), ExceptionData> {
        todo!();
        // let frame = current_frame_mut!(self)?;

        // let ip = frame.increment_ip();
        // let function = self.functions.get(&frame.function())
        //     .ok_or_else(|| self.vm_exception(VMException::InvalidFunction))?;

        // let opcode = *function.code().get(ip)
        //     .ok_or_else(|| self.vm_exception(VMException::FunctionOverrun))?;

        // self.interpret(opcode)
    }

    fn interpret(&mut self, opcode: Opcode) -> Result<(), Exception> {
        match opcode {
            Opcode::NoOp => {},

            Opcode::Jump(address) => {
                let frame = current_frame_mut!(self)?;
                frame.set_ip(address as usize)
            },
            Opcode::JumpIf(address) => {
                let x = self.pop_as()?;
                if x {
                    let frame = current_frame_mut!(self)?;
                    frame.set_ip(address as usize);
                }
            },
            Opcode::Call(arg_count) => {
                // The position in the stack the function to call resides at
                // is immediately before the arguments, so the position is
                // the current position - the amount of arguments - 1.
                let function_position = self.stack_position() - arg_count as usize - 1;
                let function = self.get_at_as::<FuncId>(function_position)?;
                
                self.call(function, arg_count, false)?;
            },
            Opcode::Ret => {
                let ret_value = self.pop()?;

                self.ret();

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
            Opcode::Swap => {
                let a = self.pop()?;
                let b = self.pop()?;
                self.push(a)?;
                self.push(b)?;
            },

            Opcode::StoreVar(var) => {
                let value = self.pop()?;
                self.set_variable(var, value)?;
            },
            Opcode::LoadVar(var) => {
                let value = self.get_variable(var)?;
                self.push(value)?;
            },

            Opcode::Add =>  self.binary_op(|a: i32, b: i32| a + b)?,
            Opcode::Sub =>  self.binary_op(|a: i32, b: i32| a - b)?,
            Opcode::Mult => self.binary_op(|a: i32, b: i32| a * b)?,
            Opcode::Div => {
                let b = self.pop_as::<i32>()?;
                let a = self.pop_as::<i32>()?;
                
                if b == 0 {
                    return Err(self.code_exception(CodeException::DivisionBy0));
                }

                let x = a / b;
                self.push(Value::Number(x))?;
            },
            Opcode::Equal => {
                let b = self.pop()?;
                let a = self.pop()?;
                let x = (a == b).to_value();

                self.push(x)?;
            },
            Opcode::LessThan =>    self.binary_op(|a: i32, b: i32| a < b)?,
            Opcode::Not =>         self.unary_op(|x: bool| !x)?,
            Opcode::And =>         self.binary_op(|a: bool, b: bool| a && b)?,
            Opcode::Or =>          self.binary_op(|a: bool, b: bool| a || b)?,
            Opcode::GreaterThan => self.binary_op(|a: i32, b: i32| a > b)?,
        }

        Ok(())
    }
}
