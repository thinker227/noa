use std::fmt::Display;

use super::opcode::FuncId;

/// A runtime value.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub enum Value {
    /// A 32-bit integer.
    Number(i32),
    /// A boolean.
    Bool(bool),
    /// A function.
    Function(FuncId),
    /// NIL / `()`
    Nil,
}

impl Display for Value {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Value::Number(x) => write!(f, "{x}"),
            Value::Bool(x) => write!(f, "{x}"),
            Value::Function(id) => write!(f, "<func {id}>"),
            Value::Nil => write!(f, "()"),
        }
    }
}
