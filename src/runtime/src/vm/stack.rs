use crate::value::Value;
use crate::exception::Exception;

/// Wrapper around a vector representing a stack of values.
#[derive(Debug, Clone)]
pub struct Stack {
    stack: Vec<Value>,
}

impl Stack {
    pub fn new(size: usize) -> Self {
        Self {
            stack: Vec::with_capacity(size)
        }
    }

    pub fn head(&self) -> usize {
        self.stack.len()
    }

    pub fn get(&self, at: usize) -> Option<&Value> {
        self.stack.get(at)
    }

    pub fn get_mut(&mut self, at: usize) -> Option<&mut Value> {
        self.stack.get_mut(at)
    }

    pub fn slice_from_end(&self, size: usize) -> Option<&[Value]> {
        self.stack.get((self.head() - size)..)
    }

    pub fn push(&mut self, value: Value) -> Result<(), Exception> {
        self.stack.push_within_capacity(value)
            .map_err(|_| Exception::StackOverflow)
    }

    pub fn pop(&mut self) -> Result<Value, Exception> {
        self.stack.pop()
            .ok_or(Exception::StackUnderflow)
    }

    pub fn shrink(&mut self, new_size: usize) {
        self.stack.truncate(new_size);
    }

    pub fn iter(&self) -> impl Iterator<Item = &Value> {
        self.stack.iter().rev()
    }
}
