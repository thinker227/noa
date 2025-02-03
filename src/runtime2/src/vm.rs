use std::cell::RefCell;

use frame::{Frame, FrameKind};
use stack::Stack;

use crate::ark::Function;
use crate::exception::{Exception, FormattedException, TraceFrame};
use crate::native::{NativeCall, NativeFunction};
use crate::value::Value;
use crate::heap::Heap;

pub mod frame;
mod interpret;
mod stack;

/// Constants for a single execution of the virtual machine.
struct VmConsts {
    pub functions: Vec<Function>,
    pub native_functions: Vec<NativeFunction>,
    pub strings: Vec<String>,
    pub code: Vec<u8>,
}

/// The vm's call stack.
struct CallStack {
    pub stack: Vec<Frame>,
}

/// The instruction pointer for the vm.
enum Ip {
    User(usize),
    Native(Box<RefCell<dyn NativeCall>>),
    None,
}

/// The runtime virtual machine.
pub struct Vm<'a> {
    consts: VmConsts,
    stack: Stack,
    heap: Heap,
    call_stack: &'a mut CallStack,
    ip: Ip,
}

/// Creates a new [`Vm`] and passes it to a closure
/// which should appropriately call into the vm to begin execution.
pub fn execute(
    functions: Vec<Function>,
    strings: Vec<String>,
    code: Vec<u8>,
    stack_size: usize,
    call_stack_size: usize,
    heap_size: usize,
    mut run: impl FnMut(&mut Vm) -> Result<Value, FormattedException>
) -> Result<Value, FormattedException> {
    let mut call_stack = CallStack {
        stack: Vec::with_capacity(call_stack_size)
    };

    let mut vm = Vm::new(
        functions,
        strings,
        code,
        stack_size,
        &mut call_stack,
        heap_size
    );

    let res = run(&mut vm);

    res
}

impl<'a> Vm<'a> {
    /// Creates a new [`Vm`].
    fn new(
        functions: Vec<Function>,
        strings: Vec<String>,
        code: Vec<u8>,
        stack_size: usize,
        call_stack: &'a mut CallStack,
        heap_size: usize
    ) -> Self {
        Self {
            consts: VmConsts {
                functions,
                native_functions: vec![],
                strings,
                code
            },
            stack: Stack::new(stack_size),
            heap: Heap::new(heap_size),
            call_stack,
            ip: Ip::None,
        }
    }

    /// Formats an [`Exception`] into a [`FormattedException`].
    fn _format_exception(&self, exception: Exception) -> FormattedException {
        let stack_trace = self._construct_stack_trace();

        FormattedException {
            exception,
            stack_trace
        }
    }

    /// Constructs a stack trace from the current call stack.
    fn _construct_stack_trace(&self) -> Vec<TraceFrame> {
        self.call_stack.stack.iter()
            .filter_map(|frame| -> Option<TraceFrame> {
                match frame.kind {
                    FrameKind::UserFunction => Some(frame.into()),
                    _ => None,
                }
            })
            // Have to reverse the stack since the latest frame, where the current execution is at,
            // sits at the very end of the call stack.
            .rev()
            .collect()
    }
}
