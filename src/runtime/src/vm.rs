use std::collections::HashMap;

use crate::ark::Ark;
use code_reader::CodeReader;
use crate::runtime::exception::{Exception, ExceptionData, StackTraceAddress, StackTraceFrame, VMException};
use crate::ark::function::Function;
use crate::ark::opcode::FuncId;
use frame::StackFrame;
use stack::Stack;
use gc::Gc;

mod code_reader;
mod stack;
mod frame;
mod interpret;
mod flow_control;
pub mod gc;

/// A virtual machine. Contains the entire state of the runtime.
#[derive(Debug)]
pub struct VM {
    functions: HashMap<FuncId, Function>,
    strings: Vec<String>,
    main: FuncId,
    call_stack: Vec<StackFrame>,
    stack: Stack,
    code: CodeReader,
    gc: Gc
}

impl VM {
    /// Constructs a new virtual machine.
    pub fn new(ark: Ark, call_stack_size: usize, stack_size: usize) -> Self {
        let main = ark.header.main;

        let mut functions = HashMap::new();
        for function in ark.function_section.functions {
            functions.insert(function.id(), function);
        }

        let strings = ark.string_section.strings;

        let call_stack = Vec::with_capacity(call_stack_size);
        let stack = Stack::new(stack_size);

        let code = CodeReader::new(ark.code_section.code);

        let gc = Gc::new();

        let vm = Self {
            functions,
            strings,
            main,
            call_stack,
            stack,
            code,
            gc
        };

        vm
    }

    /// The functions the virtual machine interprets.
    pub fn functions(&self) -> &HashMap<FuncId, Function> {
        &self.functions
    }

    /// Gets a string with a specified index.
    pub fn get_string(&self, index: u32) -> Result<&String, ExceptionData> {
        self.strings.get(index as usize)
            .ok_or_else(|| ExceptionData::VM(VMException::InvalidString))
    }

    /// Gets a string with a specified index,
    /// or returns a fallback string in case a string with the specified index
    /// does not exist.
    pub fn get_string_or_fallback<'a>(&'a self, index: u32, s: &'static str) -> &'a str {
        self.strings.get(index as usize)
            .map(|x| x.as_str())
            .unwrap_or(s)
    }

    /// The ID of the main function.
    pub fn main(&self) -> FuncId {
        self.main
    }

    /// Gets the current stack trace.
    fn get_stack_trace(&self) -> Vec<StackTraceFrame> {
        let mut trace = Vec::new();

        let mut caller_address = StackTraceAddress::Explicit(self.code.ip());
        
        for frame in self.call_stack.iter().rev() {
            trace.push(StackTraceFrame {
                function: frame.as_function().function,
                address: caller_address
            });
            
            caller_address = match frame.as_function().caller_address() {
                Some(x) => StackTraceAddress::Explicit(x.value()),
                None => StackTraceAddress::Implicit,
            }
        }

        trace
    }

    /// Creates an exception.
    fn create_exception(&self, data: ExceptionData) -> Exception {
        let stack_trace = self.get_stack_trace();
        Exception::new(data, stack_trace)
    }
}
