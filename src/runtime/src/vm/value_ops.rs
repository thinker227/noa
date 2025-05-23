use std::collections::HashMap;

use crate::value::{Closure, Type, Value};
use crate::heap::{HeapAddress, HeapValue};
use crate::exception::{Exception, FormattedException};

use super::{Vm, Result};

impl Vm {
    /// Gets the type of a value.
    /// 
    /// Might return an exception in case the value is an object which points to invalid heap memory.
    pub fn get_type(&self, val: Value) -> Result<Type> {
        match val {
            Value::Number(_) => Ok(Type::Number),
            Value::Bool(_) => Ok(Type::Bool),
            Value::InternedString(_) => Ok(Type::String),
            Value::Function(_) => Ok(Type::Function),
            Value::Object(heap_address) => match self.get_heap_value(heap_address)? {
                HeapValue::String(_) => Ok(Type::String),
                HeapValue::List(_) => Ok(Type::List),
                HeapValue::Object(_) => todo!(),
            },
            Value::Nil => Ok(Type::Nil),
        }
    }

    /// Checks whether two values are equal.
    pub fn equal(&self, a: Value, b: Value) -> Result<bool> {
        // Try checking whether both values are string-like first.
        if let (Some(a), Some(b)) = (self.try_get_string(a)?, self.try_get_string(b)?) {
            return Ok(a == b);
        }

        match (a, b) {
            (Value::Number(a), Value::Number(b)) => Ok(a == b),

            (Value::Bool(a), Value::Bool(b)) => Ok(a == b),

            (Value::InternedString(_), Value::InternedString(_)) => unreachable!(),

            (Value::Function(a), Value::Function(b)) => Ok(a == b),

            (Value::Object(a), Value::Object(b)) => match (self.get_heap_value(a)?, self.get_heap_value(b)?) {
                (HeapValue::String(_), HeapValue::String(_)) => unreachable!(),
                (HeapValue::List(_), HeapValue::List(_)) => todo!("not yet specified"),
                (HeapValue::Object(_), HeapValue::Object(_)) => todo!("not yet specified"),
                _ => Ok(false)
            },

            (Value::Nil, Value::Nil) => Ok(true),

            _ => Ok(false)
        }
    }

    /// Tries to get a string from a value without performing any coercion.
    pub fn try_get_string(&self, val: Value) -> Result<Option<String>> {
        match val {
            Value::InternedString(index) =>
                self.consts.strings.get(index)
                    .cloned()
                    .ok_or_else(|| self.exception(Exception::InvalidString(index)))
                    .map(|x| Some(x)),
            
            Value::Object(heap_addresss) => match self.get_heap_value(heap_addresss)? {
                HeapValue::String(str) => Ok(Some(str.clone())),
                _ => Ok(None)
            },

            _ => Ok(None)
        }
    }

    /// Turns a value into a string representation.
    pub fn to_string(&self, val: Value) -> Result<String> {
        match val {
            Value::Number(x) => Ok(x.to_string()),

            Value::Bool(x) => if x {
                Ok("true".to_string())
            } else {
                Ok("false".to_string())
            },

            Value::InternedString(index) => self.consts.strings.get(index)
                .cloned()
                .ok_or_else(|| self.exception(Exception::InvalidString(index))),
            
            Value::Function(closure) => {
                let id = closure.function.decode();
                let name = if closure.function.is_native() {
                    todo!("name of native functions")
                } else {
                    let name_index = self.consts.functions.get(id as usize)
                        .ok_or_else(|| self.exception(Exception::InvalidUserFunction(id)))?
                        .name_index as usize;

                    self.consts.strings.get(name_index)
                        .cloned()
                        .ok_or_else(|| self.exception(Exception::InvalidString(name_index)))?
                };
                Ok(name)
            },

            Value::Object(heap_address) => match self.get_heap_value(heap_address)? {
                HeapValue::String(str) => Ok(str.clone()),
                HeapValue::List(_) => todo!("not yet specified"),
                HeapValue::Object(_) => todo!("not yet specified"),
            },

            Value::Nil => Ok("()".to_string()),
        }
    }

    /// Tries to coerce a value into a number.
    pub fn coerce_to_number(&self, val: Value) -> Result<f64> {
        match val {
            Value::Number(x) => Ok(x),
            Value::Bool(x) => Ok(if x { 1. } else { 0. }),
            Value::Object(heap_address) => match self.get_heap_value(heap_address)? {
                HeapValue::List(_) => todo!("not specified yet"),
                HeapValue::Object(_) => todo!("not specified yet"),
                _ => Err(self.coercion_error(val, Type::Number)),
            },
            Value::Nil => Ok(0.),
            _ => Err(self.coercion_error(val, Type::Number)),
        }
    }

    /// Tries to coerce a value into a boolean.
    pub fn coerce_to_bool(&self, val: Value) -> Result<bool> {
        match val {
            Value::Number(_) => Ok(true),
            Value::Bool(x) => Ok(x),
            Value::InternedString(_) => Ok(true),
            Value::Function(_) => Ok(true),
            Value::Object(heap_address) => match self.get_heap_value(heap_address)? {
                HeapValue::String(_) => Ok(true),
                HeapValue::List(_) => todo!("not specified yet"),
                HeapValue::Object(_) => todo!("not specified yet"),
            },
            Value::Nil => Ok(false),
        }
    }

    /// Tries to coerce a value into a closure.
    pub fn coerce_to_function(&self, val: Value) -> Result<Closure> {
        match val {
            Value::Function(x) => Ok(x),
            _ => Err(self.coercion_error(val, Type::Function)),
        }
    }

    /// Tries to coerce a value into a list.
    pub fn coerce_to_list(&self, _: Value) -> Result<(&Vec<Value>, HeapAddress)> {
        todo!("not specified yet")
    }

    /// Tries to coerce a value into an object.
    pub fn coerce_to_object(&self, _: Value) -> Result<(&HashMap<String, Value>, HeapAddress)> {
        todo!("not specified yet")
    }

    // Constructs a formatted coercion error exception.
    fn coercion_error(&self, val: Value, ty: Type) -> FormattedException {
        let val = match val {
            Value::Number(_) => "a number",
            Value::Bool(_) => "a boolean",
            Value::InternedString(_) => "a string",
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
