use super::{Type, Value};

/// An error which is the result of an invalid coercion.
#[derive(Debug, PartialEq, Eq)]
pub enum CoercionError {
    /// Attempted to coerce something into a function.
    ToFunction,
    /// Attempted to coerce something to nil. This is technically impossible.
    ToNil,
    /// Attempted to coerce a function into a number.
    FunctionToNumber,
}

impl Value {
    /// Tries to coerce the value to another type.
    pub fn try_coerce(&self, to: Type) -> Result<Value, CoercionError> {
        match (*self, to) {
            (Value::Number(_), Type::Number) => Ok(*self),
            (Value::Number(_), Type::Bool) => Ok(Value::Bool(true)),
            (Value::Number(_), Type::Function) => Err(CoercionError::ToFunction),

            (Value::Bool(b), Type::Number) => Ok(Value::Number(
                if b {
                    1
                } else {
                    0
                }
            )),
            (Value::Bool(_), Type::Bool) => Ok(*self),
            (Value::Bool(_), Type::Function) => Err(CoercionError::ToFunction),

            (Value::Function(_), Type::Number) => Err(CoercionError::FunctionToNumber),
            (Value::Function(_), Type::Bool) => Ok(Value::Bool(true)),
            (Value::Function(_), Type::Function) => Ok(*self),

            (Value::Nil, Type::Number) => Ok(Value::Number(0)),
            (Value::Nil, Type::Bool) => Ok(Value::Bool(false)),
            (Value::Nil, Type::Function) => Err(CoercionError::ToFunction),

            (_, Type::Nil) => Err(CoercionError::ToNil),
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
            Value::Nil => 0,
        }
    }
}
