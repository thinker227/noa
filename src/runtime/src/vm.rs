use debugger::Debugger;
use frame::{Frame, FrameKind};
use stack::Stack;

use crate::ark::Function;
use crate::exception::{Exception, FormattedException, TraceFrame};
use crate::native::NativeFunction;
use crate::heap::{Heap, HeapAddress, HeapGetError, HeapValue};

pub mod frame;
mod interpret;
mod value_ops;
mod stack;
pub mod debugger;

/// The result of a VM operation.
pub type Result<T> = std::result::Result<T, FormattedException>;

/// Constants for a single execution of the virtual machine.
pub struct VmConsts {
    /// User functions.
    pub functions: Vec<Function>,
    /// Native functions.
    pub native_functions: Vec<NativeFunction>,
    /// Constant strings.
    pub strings: Vec<String>,
    /// Bytecode instructions.
    pub code: Vec<u8>,
}

/// The runtime virtual machine.
pub struct Vm {
    /// Immutable 'constants' for the vm execution.
    consts: VmConsts,
    /// The vm's stack memory.
    /// Home of all function arguments, local variables, and temporaries.
    stack: Stack,
    /// The vm's heap where all non-stack memory is allocated.
    heap: Heap,
    /// The vm's call stack.
    /// Responsible for keeping track of what function is currently being executed.
    call_stack: Vec<Frame>,
    /// The instruction pointer. Points to a specific byte in [`VmConsts::code`]
    /// which is the *next* bytecode instruction to be executed.
    ip: usize,
    /// The instruction pointer used as reference when constructing a stack trace.
    /// This is not the same as [`Self::ip`] since this will always point at the
    /// bytecode instruction which is *currently* being executed, to provide
    /// better traces.
    trace_ip: usize,
    /// The debugger interface.
    debugger: Option<Box<dyn Debugger>>,
}

impl Vm {
    /// Creates a new [`Vm`].
    pub fn new(
        functions: Vec<Function>,
        strings: Vec<String>,
        code: Vec<u8>,
        stack_size: usize,
        call_stack_size: usize,
        heap_size: usize,
        debugger: Option<Box<dyn Debugger + 'static>>
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
            // This is just a placeholder, the instruction pointer will be overridden once a function is called.
            ip: 0,
            trace_ip: 0,
            debugger
        }
    }

    /// Gets the debugger interface.
    pub fn debugger(&mut self) -> &mut Option<Box<dyn Debugger>> {
        &mut self.debugger
    }

    /// Gets a value at a specified address on the heap.
    fn get_heap_value(&self, address: HeapAddress) -> Result<&HeapValue> {
        self.heap.get(address)
            .map_err(|e| {
                let ex = match e {
                    HeapGetError::OutOfBounds => Exception::OutOfBoundsHeapAddress,
                    HeapGetError::SlotFreed => Exception::FreedHeapAddress,
                };
                self.exception(ex)
            })
    }

    /// Formats an [`Exception`] into a [`FormattedException`].
    fn exception(&self, exception: Exception) -> FormattedException {
        let stack_trace = self.construct_stack_trace();

        FormattedException {
            exception,
            stack_trace
        }
    }

    /// Constructs a stack trace from the current call stack.
    fn construct_stack_trace(&self) -> Vec<TraceFrame> {
        let mut stack_trace = Vec::new();

        let mut frames = self.call_stack.iter()
            .rev()
            .filter(|frame| !matches!(frame.kind, FrameKind::Temp { .. }));

        if let Some(first) = frames.next() {
            let mut address = match first.kind {
                FrameKind::UserFunction => Some(self.trace_ip),
                FrameKind::NativeFunction => None,
                FrameKind::Temp { .. } => unreachable!()
            };
            stack_trace.push(self.construct_trace_frame(first, address));
    
            let mut previous = first;
    
            for frame in frames {
                address = previous.ret;
                stack_trace.push(self.construct_trace_frame(frame, address));
                previous = frame;
            }
        }

        stack_trace.push(TraceFrame {
            function: "<execution root>".into(),
            address: None
        });

        stack_trace
    }

    fn construct_trace_frame(&self, frame: &Frame, address: Option<usize>) -> TraceFrame {
        let is_native = frame.function.is_native();
        let func_id = frame.function.decode();

        let func_name = if is_native {
            "<native function>".into() // todo: native function names
        } else {
            match self.consts.functions.get(func_id as usize) {
                Some(function) => match self.consts.strings.get(function.name_index as usize) {
                    Some(str) => str.clone(),
                    None => "<invalid string index>".into(),
                },
                None => "<invalid function index>".into(),
            }
        };

        TraceFrame {
            function: func_name,
            address
        }
    }
}
