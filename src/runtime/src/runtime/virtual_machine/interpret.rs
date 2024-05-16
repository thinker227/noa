use crate::runtime::value::Value;
use crate::runtime::opcode::{self, FuncId};
use crate::runtime::exception::{CodeException, Exception, ExceptionData, VMException};
use super::VM;

impl VM {
    /// Executes the main function.
    pub fn execute_main(&mut self) -> Result<Value, Exception> {
        self.execute_main_internal()
            .map_err(|data| {
                let stack_trace = self.get_stack_trace();
                return Exception::new(data, stack_trace);
            })
    }

    fn execute_main_internal(&mut self) -> Result<Value, ExceptionData> {
        self.call(self.main, 0, true)?;

        self.run()?;

        let ret_value = self.stack.pop()?;
        Ok(ret_value)
    }

    fn run(&mut self) -> Result<(), ExceptionData> {
        while !self.call_stack.is_empty() {
            self.interpret()?;
        }

        Ok(())
    }

    fn interpret(&mut self) -> Result<(), ExceptionData> {
        let opcode = self.code.read_byte()?;
        
        match opcode {
            opcode::NO_OP => {}

            opcode::JUMP => {
                let address = self.code.read_u32()?;
                self.code.jump(address as usize);
            }
            opcode::JUMP_IF => {
                let address = self.code.read_u32()?;

                let val = self.stack.pop_as::<bool>()?;

                if val {
                    self.code.jump(address as usize);
                }
            }

            opcode::CALL => {
                let arg_count = self.code.read_u32()?;

                let function_position = self.stack.head_position() - arg_count as usize - 1;
                let function = self.stack.get_at(function_position)
                    .expect("stack should contain enough elements to contain function")
                    .to::<FuncId>()
                    .map_err(|e| ExceptionData::Code(CodeException::CoercionError(e)))?;

                self.call(function, arg_count, false)?;
            }
            opcode::RET => {
                let ret_value = self.stack.pop()?;

                self.ret();

                self.stack.push(ret_value)?;
            }

            opcode::PUSH_INT => {
                let val = self.code.read_i32()?;
                self.stack.push(Value::Number(val))?;
            }
            opcode::PUSH_BOOL => {
                let val = self.code.read_bool()?;
                self.stack.push(Value::Bool(val))?;
            }
            opcode::PUSH_FUNC => {
                let val = self.code.read_u32()?;
                self.stack.push(Value::Function(val))?;
            }
            opcode::PUSH_NIL => {
                self.stack.push(Value::Nil)?;
            }

            opcode::POP => {
                self.stack.pop()?;
            }
            opcode::DUP => {
                let val = self.stack.pop()?;
                self.stack.push(val)?;
                self.stack.push(val)?;
            }
            opcode::SWAP => {
                let a = self.stack.pop()?;
                let b = self.stack.pop()?;

                self.stack.push(a)?;
                self.stack.push(b)?;
            }

            opcode::STORE_VAR => todo!(),
            opcode::LOAD_VAR => todo!(),

            opcode::ADD => self.stack.binary_op(|a: i32, b: i32| a + b)?,
            opcode::SUB => self.stack.binary_op(|a: i32, b: i32| a - b)?,
            opcode::MULT => self.stack.binary_op(|a: i32, b: i32| a * b)?,
            opcode::DIV => {
                let b = self.stack.pop_as::<i32>()?;
                let a = self.stack.pop_as::<i32>()?;

                if b == 0 {
                    return Err(ExceptionData::Code(CodeException::DivisionBy0));
                }

                let x = a / b;
                self.stack.push(Value::Number(x))?;
            }
            opcode::LESS_THAN => self.stack.binary_op(|a: i32, b: i32| a < b)?,
            opcode::NOT => self.stack.unary_op(|x: bool| !x)?,
            opcode::AND => self.stack.binary_op(|a: bool, b: bool| a && b)?,
            opcode::OR => self.stack.binary_op(|a: bool, b: bool| a || b)?,
            opcode::GREATER_THAN => self.stack.binary_op(|a: i32, b: i32| a > b)?,

            _ => return Err(ExceptionData::VM(VMException::InvalidOpcode))
        }

        Ok(())
    }
}
