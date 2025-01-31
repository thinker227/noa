use heap::Heap;

use crate::ark::Function;
use crate::value::Value;

pub mod heap;

/// The runtime virtual machine.
pub struct Vm {
    functions: Vec<Function>,
    strings: Vec<String>,
    stack: Vec<Value>,
    heap: Heap,
    code: Vec<u8>,
    ip: usize,
}

impl Vm {
    /// Creates a new [`Vm`].
    pub fn new(functions: Vec<Function>, strings: Vec<String>, code: Vec<u8>, heap_size: usize) -> Self {
        Self {
            functions,
            strings,
            stack: Vec::new(),
            heap: Heap::new(heap_size),
            code,
            ip: 0
        }
    }
}
