use std::fmt::Display;

use super::opcode::FuncId;

pub mod coercion;

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

/// The type of a runtime value.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub enum Type {
    /// A 32-bit integer.
    Number,
    /// A boolean.
    Bool,
    /// A function.
    Function,
    /// NIL / `()`
    Nil,
}

impl Value {
    /// Gets the type of the value.
    pub fn value_type(&self) -> Type {
        match *self {
            Value::Number(_) => Type::Number,
            Value::Bool(_) => Type::Bool,
            Value::Function(_) => Type::Function,
            Value::Nil => Type::Nil,
        }
    }

    
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
