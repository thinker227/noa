use std::collections::HashMap;

use crate::heap::HeapAddress;
use crate::ark::FuncId;

/// The type of a runtime value.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Type {
    Number,
    Bool,
    Function,
    String,
    List,
    Object,
    Nil
}

/// A closure over a function and an object containing captured variables.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Closure {
    /// The ID of the function the closure calls.
    pub function: FuncId,
    /// The object containing the captured variables for the closure,
    /// or [`None`] if the closure contains no captured variables.
    pub captures: Option<HeapAddress>,
}

impl From<FuncId> for Closure {
    fn from(value: FuncId) -> Self {
        Self {
            function: value,
            captures: None
        }
    }
}

/// The 'location' of a string within the vm.
pub enum StringLocation {
    /// An interned string with an index pointing into the string section.
    Interned(usize),
    /// A string allocated on the heap with an address on the heap.
    Allocated(HeapAddress),
}

/// A list.
#[derive(Debug, Clone, PartialEq)]
pub struct List(pub Vec<Value>);

/// An object.
#[derive(Debug, Clone, PartialEq)]
pub struct Object {
    pub fields: HashMap<String, Field>,
    pub dynamic: bool,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Field {
    pub val: Value,
    pub mutable: bool,
    pub index: u32,
}

/// A runtime value.
#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Value {
    /// A number.
    Number(f64),
    /// A boolean.
    Bool(bool),
    /// A reference to a string in the string section.
    InternedString(usize),
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

impl From<usize> for Value {
    fn from(value: usize) -> Self {
        Self::Number(value as f64)
    }
}

impl From<bool> for Value {
    fn from(value: bool) -> Self {
        Self::Bool(value)
    }
}

impl From<StringLocation> for Value {
    fn from(value: StringLocation) -> Self {
        match value {
            StringLocation::Interned(index) => Self::InternedString(index),
            StringLocation::Allocated(heap_address) => Self::Object(heap_address),
        }
    }
}

impl From<Closure> for Value {
    fn from(value: Closure) -> Self {
        Self::Function(value)
    }
}

impl From<HeapAddress> for Value {
    fn from(value: HeapAddress) -> Self {
        Self::Object(value)
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

impl<'a> TryFrom<&'a Value> for &'a Closure {
    type Error = ();

    fn try_from(value: &'a Value) -> Result<Self, Self::Error> {
        match value {
            Value::Function(x) => Ok(x),
            _ => Err(())
        }
    }
}

impl TryFrom<&Value> for HeapAddress {
    type Error = ();

    fn try_from(value: &Value) -> Result<Self, Self::Error> {
        match value {
            Value::Object(x) => Ok(*x),
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
