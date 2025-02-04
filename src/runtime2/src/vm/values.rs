use crate::{exception::{Exception, FormattedException}, heap::HeapValue, value::{Type, Value}};

use super::{Vm, Result};

impl Vm<'_> {
    /// Gets the type of a value.
    /// 
    /// Might return an exception in case the value is an object which points to invalid heap memory.
    pub fn get_type(&self, val: Value) -> Result<Type> {
        match val {
            Value::Number(_) => Ok(Type::Number),
            Value::Bool(_) => Ok(Type::Bool),
            Value::Function(_) => Ok(Type::Function),
            Value::Object(heap_address) => match self.get_heap_value(heap_address)? {
                HeapValue::String(_) => Ok(Type::String),
                HeapValue::List(_) => Ok(Type::List),
                HeapValue::Object(_) => todo!(),
            },
            Value::Nil => Ok(Type::Nil),
        }
    }

    /// Coerces a value into another type according to value coercion rules.
    pub fn coerce(&self, val: Value, ty: Type) -> Result<Value> {
        let error = || self.coercion_error(val, ty);

        match (val, ty) {
            (Value::Number(x), Type::Number) => Ok(x.into()),
            (Value::Number(_), Type::Bool)        => Ok(true.into()),
            (Value::Number(_), Type::List)        => todo!("not specified yet"),
            (Value::Number(_), _)                 => Err(error()),

            (Value::Bool(x), Type::Number) => Ok(if x { 1.0.into() } else { 0.0.into() }),
            (Value::Bool(x), Type::Bool)   => Ok(x.into()),
            (Value::Bool(_), Type::List)         => todo!("not specified yet"),
            (Value::Bool(_), _)                  => Err(error()),

            (Value::Function(x), Type::Function) => Ok(x.into()),
            (Value::Function(_), Type::List)              => todo!("not specified yet"),
            (Value::Function(_), _)                       => Err(error()),

            (Value::Object(heap_address), ty) => match (self.get_heap_value(heap_address)?, ty) {
                (HeapValue::String(_), Type::Bool)   => Ok(true.into()),
                (HeapValue::String(_), Type::String) => Ok(heap_address.into()),
                (HeapValue::String(_), Type::List)   => todo!("not specified yet"),
                (HeapValue::String(_), _)            => Err(error()),

                (HeapValue::List(_), Type::Number)   => todo!("not specified yet"),
                (HeapValue::List(_), Type::Bool)     => todo!("not specified yet"),
                (HeapValue::List(_), Type::Function) => todo!("not specified yet"),
                (HeapValue::List(_), Type::String)   => todo!("not specified yet"),
                (HeapValue::List(_), Type::List)     => todo!("not specified yet"),
                (HeapValue::List(_), _)              => todo!("not specified yet"),

                (HeapValue::Object(_), Type::Number)   => todo!("not specified yet"),
                (HeapValue::Object(_), Type::Bool)     => todo!("not specified yet"),
                (HeapValue::Object(_), Type::Function) => todo!("not specified yet"),
                (HeapValue::Object(_), Type::String)   => todo!("not specified yet"),
                (HeapValue::Object(_), Type::List)     => todo!("not specified yet"),
                (HeapValue::Object(_), _)              => todo!("not specified yet"),
            },

            (Value::Nil, Type::Number) => Ok(0.0.into()),
            (Value::Nil, Type::Bool)   => Ok(false.into()),
            (Value::Nil, Type::String) => todo!("not specified yet"),
            (Value::Nil, Type::List)   => todo!("not specified yet"),
            (Value::Nil, _)            => Err(error()),
        }
    }

    // Constructs a formatted coercion error exception.
    fn coercion_error(&self, val: Value, ty: Type) -> FormattedException {
        let val = match val {
            Value::Number(_) => "a number",
            Value::Bool(_) => "a boolean",
            Value::Function(_) => "a function",
            Value::Object(heap_address) => match self.heap.get(heap_address) {
                Ok(HeapValue::String(_)) => "a string",
                Ok(HeapValue::List(_)) => "a list",
                Ok(HeapValue::Object(_)) => "a object",
                Err(_) => "an invalid heap address",
            },
            Value::Nil => "()",
        };

        let ty = match ty {
            Type::Number => "a number",
            Type::Bool => "a boolean",
            Type::Function => "a function",
            Type::String => "a string",
            Type::List => "a list",
            Type::Nil => "()",
        };

        self.exception(Exception::CoercionError(val.into(), ty.into()))
    }
}
