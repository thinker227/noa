use crate::vm::heap::HeapAddress;
use crate::ark::FuncId;

/// The type of a runtime value.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Type {
    Number,
    Bool,
    Function,
    String,
    List,
    Nil
}

/// A closure over a function and an object containing captured variables.
#[derive(Debug)]
pub struct Closure {
    pub function: FuncId,
    pub object: HeapAddress,
}

/// A runtime value.
#[derive(Debug)]
pub enum Value {
    /// A number.
    Number(f64),
    /// A boolean.
    Bool(bool),
    /// A function.
    Function(Closure),
    /// A heap-allocated object.
    Object(HeapAddress),
    /// `()`
    Nil
}

impl From<f64> for Value {
    fn from(value: f64) -> Self {
        Self::Number(value)
    }
}

impl From<bool> for Value {
    fn from(value: bool) -> Self {
        Self::Bool(value)
    }
}

impl From<()> for Value {
    fn from(_: ()) -> Self {
        Self::Nil
    }
}

impl TryFrom<&Value> for f64 {
    type Error = ();

    fn try_from(value: &Value) -> Result<Self, Self::Error> {
        match value {
            Value::Number(n) => Ok(*n),
            _ => Err(())
        }
    }
}

impl TryFrom<&Value> for bool {
    type Error = ();

    fn try_from(value: &Value) -> Result<Self, Self::Error> {
        match value {
            Value::Bool(x) => Ok(*x),
            _ => Err(())
        }
    }
}

impl TryFrom<&Value> for () {
    type Error = ();

    fn try_from(value: &Value) -> Result<Self, Self::Error> {
        match value {
            Value::Nil => Ok(()),
            _ => Err(())
        }
    }
}
