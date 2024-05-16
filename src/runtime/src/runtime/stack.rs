use super::value::{FromValue, Value};
use super::exception::{CodeException, ExceptionData, VMException};

#[derive(Debug)]
pub struct Stack {
    stack: Vec<Value>
}

impl Stack {
    pub fn new(stack_size: usize) -> Self {
        Self {
            stack: Vec::with_capacity(stack_size)
        }
    }

    /// Returns the current head position on the stack from the bottom of the stack.
    pub fn head_position(&self) -> usize {
        self.stack.len()
    }

    /// Clears the stack to contain only the amount of elements specified.
    pub fn clear_to(&mut self, to: usize) {
        self.stack.truncate(to);
    }

    /// Pushes a value onto the stack.
    pub fn push(&mut self, value: Value) -> Result<(), ExceptionData> {
        if self.stack.len() >= self.stack.capacity() {
            return Err(ExceptionData::VM(VMException::StackOverflow));
        }

        self.stack.push(value);

        Ok(())
    }

    /// Pops a value from the stack and returns it.
    pub fn pop(&mut self) -> Result<Value, ExceptionData> {
        self.stack.pop()
            .ok_or(ExceptionData::VM(VMException::StackUnderflow))
    }

    /// Pops a value from the stack as a specified type.
    pub fn pop_as<T: FromValue>(&mut self) -> Result<T, ExceptionData> {
        T::from_value(self.pop()?)
            .map_err(|e| ExceptionData::Code(CodeException::CoercionError(e)))
    }

    /// Gets a value at a specified position in the stack.
    pub fn get_at(&self, at: usize) -> Result<Value, ExceptionData> {
        let value = self.stack.get(at)
            .ok_or_else(|| ExceptionData::VM(VMException::StackUnderflow))?;

        Ok(*value)
    }

    /// Gets a value at a specified position in a stack as a specified type.
    pub fn get_at_as<T: FromValue>(&self, at: usize) -> Result<T, ExceptionData> {
        T::from_value(self.get_at(at)?)
            .map_err(|e| ExceptionData::Code(CodeException::CoercionError(e)))
    }

    /// Pops a value from the stack,
    /// performs a unary operation on it,
    /// and pushes the result back onto the stack.
    pub fn unary_op<T: FromValue, U: FromValue>(&mut self, op: impl FnOnce(T) -> U) -> Result<(), ExceptionData> {
        let operand = self.pop_as()?;
        let result = op(operand);

        self.push(result.to_value())?;

        Ok(())
    }

    /// Pops two values from the stack,
    /// performs a binary operation on them,
    /// and pushes the result back onto the stack.
    pub fn binary_op<T: FromValue, U: FromValue>(&mut self, op: impl FnOnce(T, T) -> U) -> Result<(), ExceptionData>
    {
        let b = self.pop_as()?;
        let a = self.pop_as()?;
        let x = op(a, b);

        self.push(x.to_value())?;

        Ok(())
    }
}
