use crate::runtime::value::{FromValue, Value};
use crate::runtime::exception::{Exception, ExceptionKind};
use super::VM;

impl VM {
    /// Pushes a value onto the stack.
    pub(super) fn push(&mut self, value: Value) -> Result<(), Exception> {
        if self.stack.len() >= self.stack.capacity() {
            return Err(Exception::new(ExceptionKind::StackOverflow));
        }

        self.stack.push(value);

        Ok(())
    }

    /// Pops a value from the stack and returns it.
    pub(super) fn pop(&mut self) -> Result<Value, Exception> {
        self.stack.pop()
            .ok_or_else(|| Exception::new(ExceptionKind::StackUnderflow))
    }

    /// Pops a value from the stack as a specified type.
    pub(super) fn pop_as<T: FromValue>(&mut self) -> Result<T, Exception> {
        T::from_value(self.pop()?)
            .map_err(|e| Exception::new(ExceptionKind::CoercionError(e)))
    }

    /// Pops a value from the stack,
    /// performs a unary operation on it,
    /// and pushes the result back onto the stack.
    pub(super) fn unary_op<T: FromValue, U: FromValue>(&mut self, op: impl FnOnce(T) -> U) -> Result<(), Exception> {
        let operand = self.pop_as()?;
        let result = op(operand);

        self.push(result.to_value())?;

        Ok(())
    }

    /// Pops two values from the stack,
    /// performs a binary operation on them,
    /// and pushes the result back onto the stack.
    pub(super) fn binary_op<T: FromValue, U: FromValue>(&mut self, op: impl FnOnce(T, T) -> U) -> Result<(), Exception>
    {
        let b = self.pop_as()?;
        let a = self.pop_as()?;
        let x = op(a, b);

        self.push(x.to_value())?;

        Ok(())
    }
}