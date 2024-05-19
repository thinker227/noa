use crate::runtime::value::Value;
use crate::runtime::opcode::{self, FuncId, VarIndex};
use crate::runtime::exception::{CodeException, Exception, ExceptionData, VMException};
use super::VM;

impl VM {
    /// Executes the main function.
    pub fn execute_main(&mut self) -> Result<Value, Exception> {
        self.execute_main_internal()
            .map_err(|data| self.create_exception(data))
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
                self.stack.push(Value::Function(FuncId::from(val)))?;
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

            opcode::STORE_VAR => {
                let var = self.code.read_u32()?;

                let value = self.stack.pop()?;

                self.set_variable(VarIndex::from(var), value)?;
            }
            opcode::LOAD_VAR => {
                let var = self.code.read_u32()?;

                let value = self.get_variable(VarIndex::from(var))?;

                self.stack.push(value)?;
            }

            opcode::ADD => self.stack.binary_op_checked(i32::checked_add)?,
            opcode::SUB => self.stack.binary_op_checked(i32::checked_sub)?,
            opcode::MULT => self.stack.binary_op_checked(i32::checked_mul)?,
            opcode::DIV => {
                let b = self.stack.pop_as::<i32>()?;
                let a = self.stack.pop_as::<i32>()?;

                if b == 0 {
                    return Err(ExceptionData::Code(CodeException::DivisionBy0));
                }

                let x = i32::checked_div(a, b)
                    .ok_or(ExceptionData::Code(CodeException::IntegerOverflow))?;
                self.stack.push(Value::Number(x))?;
            }
            opcode::EQUAL => {
                let b = self.stack.pop()?;
                let a = self.stack.pop()?;

                let val = a == b;

                self.stack.push(Value::Bool(val))?;
            }
            opcode::LESS_THAN => self.stack.binary_op(|a: i32, b: i32| a < b)?,
            opcode::NOT => self.stack.unary_op(|x: bool| !x)?,
            opcode::AND => self.stack.binary_op(|a: bool, b: bool| a && b)?,
            opcode::OR => self.stack.binary_op(|a: bool, b: bool| a || b)?,
            opcode::GREATER_THAN => self.stack.binary_op(|a: i32, b: i32| a > b)?,

            opcode::BOUNDARY => return Err(ExceptionData::VM(VMException::FunctionOverrun)),

            _ => return Err(ExceptionData::VM(VMException::InvalidOpcode))
        }

        Ok(())
    }

    fn get_variable(&self, variable: VarIndex) -> Result<Value, ExceptionData> {
        let stack_index = self.get_variable_stack_index(variable);

        let val = self.stack.get_at(stack_index)
            .ok_or(ExceptionData::VM(VMException::InvalidVariable))?;

        Ok(val)
    }

    fn set_variable(&mut self, variable: VarIndex, value: Value) -> Result<(), ExceptionData> {
        let stack_index = self.get_variable_stack_index(variable);

        let val = self.stack.get_at_mut(stack_index)
            .ok_or(ExceptionData::VM(VMException::InvalidVariable))?;

        *val = value;

        Ok(())
    }

    fn get_variable_stack_index(&self, var: VarIndex) -> usize {
        let frame = self.call_stack.last()
            .expect("call stack should not be empty");

        frame.stack_start() + var.index() as usize
    }
}
