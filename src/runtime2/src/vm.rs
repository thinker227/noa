use frame::Frame;

use crate::ark::Function;
use crate::value::Value;
use crate::heap::Heap;

pub mod frame;
mod interpret;

/// The runtime virtual machine.
pub struct Vm {
    functions: Vec<Function>,
    strings: Vec<String>,
    code: Vec<u8>,
    stack: Vec<Value>,
    heap: Heap,
    call_stack: Vec<Frame>,
    ip: usize,
}

impl Vm {
    /// Creates a new [`Vm`].
    pub fn new(functions: Vec<Function>, strings: Vec<String>, code: Vec<u8>, heap_size: usize) -> Self {
        Self {
            functions,
            strings,
            code,
            stack: Vec::new(),
            heap: Heap::new(heap_size),
            call_stack: Vec::new(),
            ip: 0
        }
    }
}
