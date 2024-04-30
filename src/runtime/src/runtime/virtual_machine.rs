use std::collections::HashMap;

use crate::ark::Ark;

use super::function::Function;
use super::opcode::FuncId;
use super::frame::StackFrame;
use super::value::Value;

mod stack;
mod interpret;

/// A virtual machine. Contains the entire state of the runtime.
#[derive(Debug)]
pub struct VM {
    functions: HashMap<FuncId, Function>,
    main: FuncId,
    call_stack: Vec<StackFrame>,
    stack: Vec<Value>,
}

impl VM {
    /// Constructs a new virtual machine.
    pub fn new(ark: Ark, call_stack_size: usize, stack_size: usize) -> Self {
        let main = ark.header.main;

        let mut functions = HashMap::new();
        for function in ark.function_section.functions {
            functions.insert(function.id(), function);
        }

        let call_stack = Vec::with_capacity(call_stack_size);
        let stack = Vec::with_capacity(stack_size);

        let vm = Self {
            functions,
            main,
            call_stack,
            stack,
        };

        vm
    }
}

/// Returns the current stack frame in the [VM].
#[macro_export]
macro_rules! current_frame {
    ($vm:expr) => {
        $vm.call_stack.last_mut()
            .ok_or_else(|| $crate::runtime::exception::Exception::new($crate::runtime::exception::ExceptionKind::NoStackFrame))
    };
}
