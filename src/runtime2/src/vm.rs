use frame::{Frame, FrameKind};

use crate::ark::Function;
use crate::exception::{Exception, FormattedException, TraceFrame};
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
                    FrameKind::Function => Some(frame.into()),
                    _ => None,
                }
            })
            // Have to reverse the stack since the latest frame, where the current execution is at,
            // sits at the very end of the call stack.
            .rev()
            .collect()
    }
}
