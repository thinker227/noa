use std::collections::HashMap;

use super::function::Function;
use super::opcode::FuncId;
use super::frame::StackFrame;
use super::value::Value;

/// A virtual machine. Contains the entire state of the runtime.
#[derive(Debug)]
pub struct VM<'a> {
    _functions: HashMap<FuncId, Function>,
    _main: FuncId,
    _call_stack: Vec<StackFrame<'a>>,
    _stack: Vec<Value>,
}

pub enum VMInitError {
    
}

impl<'a> VM<'a> {
    /// Attempts to construct a new virtual machine,
    /// or returns a [`VMInitError`] the sumbitted bytecode is invalid.
    pub fn new(mut _bytecode: &[u8]) -> Result<Self, VMInitError> {
        todo!()
    }
}
