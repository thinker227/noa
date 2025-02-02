use frame::{Frame, FrameKind};
use stack::Stack;

use crate::ark::Function;
use crate::exception::{Exception, FormattedException, TraceFrame};
use crate::native::NativeFunction;
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

/// The runtime virtual machine.
pub struct Vm {
    consts: VmConsts,
    stack: Stack,
    heap: Heap,
    call_stack: Vec<Frame>,
    ip: usize,
}

impl Vm {
    /// Creates a new [`Vm`].
    pub fn new(
        functions: Vec<Function>,
        strings: Vec<String>,
        code: Vec<u8>,
        stack_size: usize,
        call_stack_size: usize,
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
            call_stack: Vec::with_capacity(call_stack_size),
            ip: 0
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
        self.call_stack.iter()
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
