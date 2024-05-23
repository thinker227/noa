use std::fmt::Display;

use super::{Type, Value};

/// An error which is the result of an invalid coercion.
#[derive(Debug, PartialEq, Eq)]
pub struct CoercionError {
    from: Type,
    to: Type,
}

impl CoercionError {
    /// Constructs a new coercion error.
    pub fn new(from: Type, to: Type) -> Self {
        Self {
            from,
            to
        }
    }

    /// Gets the type the coercion was from.
    pub fn from(&self) -> Type {
        self.from
    }

    /// Gets the type the coercion was to.
    pub fn to(&self) -> Type {
        self.to
    }
}

impl Display for CoercionError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "cannot convert from {0} to {1}", self.from, self.to)
    }
}

impl Value {
    /// Tries to coerce the value to another type.
    pub fn try_coerce(&self, to: Type) -> Result<Value, CoercionError> {
        let err = || Err(CoercionError::new(self.value_type(), to));

        match (*self, to) {
            (Value::Number(_), Type::Number) => Ok(*self),
            (Value::Number(_), Type::Bool) => Ok(Value::Bool(true)),
            (Value::Number(_), Type::Function) => err(),
            (Value::Number(_), Type::String) => err(),

            (Value::Bool(b), Type::Number) => Ok(Value::Number(
                if b {
                    1
                } else {
                    0
                }
            )),
            (Value::Bool(_), Type::Bool) => Ok(*self),
            (Value::Bool(_), Type::Function) => err(),
            (Value::Bool(_), Type::String) => err(),

            (Value::Function(_), Type::Number) => err(),
            (Value::Function(_), Type::Bool) => Ok(Value::Bool(true)),
            (Value::Function(_), Type::Function) => Ok(*self),
            (Value::Function(_), Type::String) => err(),

            (Value::Object(_), Type::Number) => err(),
            (Value::Object(_), Type::Bool) => Ok(Value::Bool(true)),
            (Value::Object(_), Type::Function) => err(),
            (Value::Object(_), Type::String) => Ok(*self),

            (Value::Nil, Type::Number) => Ok(Value::Number(0)),
            (Value::Nil, Type::Bool) => Ok(Value::Bool(false)),
            (Value::Nil, Type::Function) => err(),
            (Value::Nil, Type::String) => err(),

            (_, Type::Nil) => err(),
        }
    }

    /// Coerces the value to an exit code.
    pub fn exit_code(&self) -> i32 {
        match *self {
            Value::Number(x) => x,
            Value::Bool(x) => if x {
                0
            } else {
                1
            },
            Value::Function(_) => 0,
            Value::Object(_) => 0,
            Value::Nil => 0,
        }
    }
}
