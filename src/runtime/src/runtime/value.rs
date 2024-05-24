use std::fmt::Display;

use crate::vm::gc::{GcRef, Spy, Trace};
use crate::ark::opcode::FuncId;
use coercion::CoercionError;

pub mod coercion;
pub mod object;

/// The type of a runtime value.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub enum Type {
    /// A 32-bit integer.
    Number,
    /// A boolean.
    Bool,
    /// A function.
    Function,
    /// A string.
    /// A string is represented by a [Value::Object] which points to a [StringObject].
    String,
    /// NIL / `()`
    Nil,
}

impl Display for Type {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match *self {
            Self::Number => write!(f, "number"),
            Self::Bool => write!(f, "bool"),
            Self::Function => write!(f, "function"),
            Self::String => write!(f, "string"),
            Self::Nil => write!(f, "()"),
        }
    }
}

/// A runtime value.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub enum Value {
    /// A 32-bit integer.
    Number(i32),
    /// A boolean.
    Bool(bool),
    /// A function.
    Function(FuncId),
    /// A managed object.
    // Todo: use an actual object trait here.
    Object(GcRef<object::StringObject>),
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
            Value::Object(obj) => todo!(),
            Value::Nil => Type::Nil,
        }
    }

    /// Tries to convert the value into another type.
    pub fn to<T: FromValue>(self) -> Result<T, CoercionError> {
        T::from_value(self)
    }
}

/// Defines types which can be converted to and from [`Value`].
pub trait FromValue where Self: Sized {
    /// Tries to convert a [`Value`] into the type.
    fn from_value(value: Value) -> Result<Self, CoercionError>;

    /// Converts the value into a [`Value`].
    fn to_value(self) -> Value;
}

impl FromValue for i32 {
    fn from_value(value: Value) -> Result<i32, CoercionError> {
        match value.try_coerce(Type::Number)? {
            Value::Number(x) => Ok(x),
            _ => unreachable!(),
        }
    }

    fn to_value(self) -> Value {
        Value::Number(self)
    }
}

impl FromValue for bool {
    fn from_value(value: Value) -> Result<bool, CoercionError> {
        match value.try_coerce(Type::Bool)? {
            Value::Bool(x) => Ok(x),
            _ => unreachable!(),
        }
    }

    fn to_value(self) -> Value {
        Value::Bool(self)
    }
}

impl FromValue for FuncId {
    fn from_value(value: Value) -> Result<FuncId, CoercionError> {
        match value.try_coerce(Type::Function)? {
            Value::Function(id) => Ok(id),
            _ => unreachable!(),
        }
    }

    fn to_value(self) -> Value {
        Value::Function(self)
    }
}

impl FromValue for () {
    fn from_value(value: Value) -> Result<(), CoercionError> {
        match value.try_coerce(Type::Nil)? {
            Value::Nil => Ok(()),
            _ => unreachable!(),
        }
    }

    fn to_value(self) -> Value {
        Value::Nil
    }
}

impl Display for Value {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Value::Number(x) => write!(f, "{x}"),
            Value::Bool(x) => write!(f, "{x}"),
            Value::Function(id) => write!(f, "<func {id}>"),
            Value::Object(_) => write!(f, "<object>"),
            Value::Nil => write!(f, "()"),
        }
    }
}

impl Trace for Value {
    fn trace(&mut self, spy: &Spy) {
        match self {
            Self::Object(r) => spy.visit(r),
            _ => {},
        }
    }
}
