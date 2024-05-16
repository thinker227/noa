use std::collections::HashMap;

use crate::ark::Ark;

use super::code_reader::CodeReader;
use super::exception::{CodeException, Exception, StackTraceFrame, VMException};
use super::function::Function;
use super::opcode::FuncId;
use super::frame::StackFrame;
use super::stack::Stack;

mod interpret;
mod flow_control;

/// A virtual machine. Contains the entire state of the runtime.
#[derive(Debug)]
pub struct VM {
    functions: HashMap<FuncId, Function>,
    strings: Vec<String>,
    main: FuncId,
    call_stack: Vec<StackFrame>,
    stack: Stack,
    code: CodeReader,
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

        let vm = Self {
            functions,
            strings,
            main,
            call_stack,
            stack,
            code,
        };

        vm
    }

    /// The functions the virtual machine interprets.
    pub fn functions(&self) -> &HashMap<FuncId, Function> {
        &self.functions
    }

    /// Gets a string with a specified index.
    pub fn get_string(&self, index: u32) -> Result<&String, Exception> {
        self.strings.get(index as usize)
            .ok_or_else(|| self.vm_exception(VMException::InvalidString))
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

        let mut return_address = self.code.ip();
        
        for frame in self.call_stack.iter().rev() {
            trace.push(StackTraceFrame {
                function: frame.function(),
                address: return_address as u32
            });
            
            return_address = frame.return_address();
        }

        trace
    }

    /// Creates a code exception.
    pub fn code_exception(&self, ex: CodeException) -> Exception {
        let stack_trace = self.get_stack_trace();
        Exception::code(ex, stack_trace)
    }

    /// Creates a VM exception.
    pub fn vm_exception(&self, ex: VMException) -> Exception {
        let stack_trace = self.get_stack_trace();
        Exception::vm(ex, stack_trace)
    }
}

/// Returns a mutable reference to the current stack frame in the [VM].
#[macro_export]
macro_rules! current_frame_mut {
    ($vm:expr) => {
        // Can't use ok_or_else here
        match $vm.call_stack.last_mut() {
            Some(x) => Ok(x),
            None => Err($vm.vm_exception($crate::runtime::exception::VMException::NoStackFrame))
        }
    };
}

/// Returns a reference to the current stack frame in the [VM].
#[macro_export]
macro_rules! current_frame {
    ($vm:expr) => {
        $vm.call_stack.last()
            .ok_or_else(|| $vm.vm_exception($crate::runtime::exception::VMException::NoStackFrame))
    };
}
