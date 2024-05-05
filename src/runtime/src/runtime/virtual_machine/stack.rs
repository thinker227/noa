use crate::current_frame;
use crate::runtime::opcode::VarIndex;
use crate::runtime::value::{FromValue, Value};
use crate::runtime::exception::{Exception, ExceptionKind};
use super::VM;

impl VM {
    /// Returns the current position on the stack from the bottom of the stack.
    pub(super) fn stack_position(&self) -> usize {
        self.stack.len()
    }

    /// Gets the value of a variable.
    pub(super) fn get_variable(&self, variable: VarIndex) -> Result<Value, Exception> {
        let frame = current_frame!(self)?;

        let stack_index = frame.stack_start() + variable as usize;

        let var = self.stack.get(stack_index)
            .ok_or_else(|| Exception::new(ExceptionKind::InvalidVariable))?;

        Ok(*var)
    }

    /// Sets the value of a variable.
    pub(super) fn set_variable(&mut self, variable: VarIndex, value: Value) -> Result<(), Exception> {
        let frame = current_frame!(self)?;

        let stack_index = frame.stack_start() + variable as usize;

        let var = self.stack.get_mut(stack_index)
            .ok_or_else(|| Exception::new(ExceptionKind::InvalidVariable))?;

        *var = value;

        Ok(())
    }

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

    /// Gets a value at a specified position in the stack.
    pub(super) fn get_at(&self, at: usize) -> Result<Value, Exception> {
        let value = self.stack.get(at)
            .ok_or_else(|| Exception::new(ExceptionKind::StackUnderflow))?;

        Ok(*value)
    }

    /// Gets a value at a specified position in a stack as a specified type.
    pub(super) fn get_at_as<T: FromValue>(&self, at: usize) -> Result<T, Exception> {
        T::from_value(self.get_at(at)?)
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
