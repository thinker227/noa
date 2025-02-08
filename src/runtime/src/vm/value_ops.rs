use std::collections::HashMap;

use crate::value::{Closure, StringLocation, Type, Value};
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

    /// Tries to coerce a value into a string.
    pub fn coerce_to_string(&self, val: Value) -> Result<(&String, StringLocation)> {
        match val {
            Value::InternedString(index) => match self.consts.strings.get(index) {
                Some(x) => Ok((x, StringLocation::Interned(index))),
                None => Err(self.exception(Exception::InvalidString(index))),
            },
            Value::Object(heap_address) => match self.get_heap_value(heap_address)? {
                HeapValue::String(x) => Ok((x, StringLocation::Allocated(heap_address))),
                HeapValue::List(_) => todo!("not yet specified"),
                HeapValue::Object(_) => todo!("not yet specified"),
            },
            _ => Err(self.coercion_error(val, Type::String)),
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

    /// Coerces a value into another type according to value coercion rules.
    pub fn coerce(&self, val: Value, ty: Type) -> Result<Value> {
        match ty {
            Type::Number   => self.coerce_to_number(val)  .map(|x| x.into()),
            Type::Bool     => self.coerce_to_bool(val)    .map(|x| x.into()),
            Type::Function => self.coerce_to_function(val).map(|x| x.into()),
            Type::String   => self.coerce_to_string(val)  .map(|(_, location)| location.into()),
            Type::List     => self.coerce_to_list(val)    .map(|(_, adr)| adr.into()),
            Type::Nil      => Err(self.coercion_error(val, Type::Nil)),
        }
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
