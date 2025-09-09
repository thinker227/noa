use polonius_the_crab::{polonius, polonius_return};

use crate::value::{Closure, Object, Type, Value};
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
                HeapValue::Object { .. } => Ok(Type::Object),
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
                (HeapValue::Object(a), HeapValue::Object(b)) => self.object_equal(a, b),
                _ => Ok(false)
            },

            (Value::Nil, Value::Nil) => Ok(true),

            _ => Ok(false)
        }
    }

    fn object_equal(&self, a: &Object, b: &Object) -> Result<bool> {
        let a = &a.fields;
        let b = &b.fields;

        if a.len() != b.len() {
            return Ok(false);
        }

        // Todo: this doesn't account for recursive objects.

        for (name, field_a) in a.iter() {
            let field_b = match b.get(name) {
                Some(x) => x,
                None => return Ok(false)
            };

            if !self.equal(field_a.val, field_b.val)? {
                return Ok(false);
            }
        }

        Ok(true)
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

                HeapValue::Object(Object { fields, dynamic }) => {
                    let mut str = String::new();
                    
                    if *dynamic {
                        str.push_str("dyn ");
                    }
                    str.push_str("{");

                    let mut fields = fields.iter().collect::<Vec<_>>();
                    fields.sort_by_key(|(_, field)| field.index);

                    let mut i = 0;
                    for (field_name, field) in fields {

                        if i >= 1 {
                            str.push_str(",");
                        }

                        // Todo: this doesn't account for recursive objects.
                        
                        let value_str = self.to_string(field.val)?;
                        str.push_str(format!(" \"{}\": {}", field_name, value_str).as_str());

                        i += 1;
                    }

                    if i >= 1 {
                        str.push(' ');
                    }
                    str.push('}');
                    
                    Ok(str)
                },
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
                HeapValue::Object { .. } => Ok(true),
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
    pub fn coerce_to_object(&self, val: Value) -> Result<(&Object, HeapAddress)> {
        match val {
            Value::Object(adr) => match self.get_heap_value(adr)? {
                HeapValue::Object(obj) => return Ok((obj, adr)),
                _ => {}
            },
            _ => {}
        };

        Err(self.coercion_error(val, Type::Object))
    }

    /// Tries to coerce a value into an object mutably.
    pub fn coerce_to_object_mut(&mut self, val: Value) -> Result<(&mut Object, HeapAddress)> {
        // See Vm::get_heap_value_mut for the reasoning behind using Polonius here.

        let mut this = self;

        polonius!(|this| -> Result<(&'polonius mut Object, HeapAddress)> {
            match val {
                // Can't use ? operator here so have to manually match.
                Value::Object(adr) => match this.get_heap_value_mut(adr) {
                    Ok(x) => match x {
                        HeapValue::Object(obj) => polonius_return!(Ok((obj, adr))),
                        _ => {}
                    },
                    Err(e) => polonius_return!(Err(e)),
                }
                _ => {}
            };
        });

        Err(this.coercion_error(val, Type::Object))
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
                Ok(HeapValue::Object { .. }) => "an object",
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
            Type::Object => "an object",
            Type::Nil => "()",
        };

        self.exception(Exception::CoercionError(val.into(), ty.into()))
    }
}
