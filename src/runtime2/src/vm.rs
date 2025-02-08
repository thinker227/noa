use frame::{Frame, FrameKind};
use stack::Stack;

use crate::ark::Function;
use crate::exception::{Exception, FormattedException, TraceFrame};
use crate::native::NativeFunction;
use crate::value::Value;
use crate::heap::{Heap, HeapAddress, HeapGetError, HeapValue};

pub mod frame;
mod interpret;
mod value_ops;
mod stack;

type Result<T> = std::result::Result<T, FormattedException>;

/// Constants for a single execution of the virtual machine.
struct VmConsts {
    /// User functions.
    pub functions: Vec<Function>,
    /// Native functions.
    pub native_functions: Vec<NativeFunction>,
    /// Constant strings.
    pub strings: Vec<String>,
    /// Bytecode instructions.
    pub code: Vec<u8>,
}

/// The vm's call stack.
struct CallStack {
    pub stack: Vec<Frame>,
}

/// The runtime virtual machine.
pub struct Vm<'a> {
    /// Immutable 'constants' for the vm execution.
    consts: VmConsts,
    /// The vm's stack memory.
    /// Home of all function arguments, local variables, and temporaries.
    stack: Stack,
    /// The vm's heap where all non-stack memory is allocated.
    heap: Heap,
    /// The vm's call stack.
    /// Responsible for keeping track of what function is currently being executed.
    call_stack: &'a mut CallStack,
    /// The instruction pointer. Points to a specific byte in [`VmConsts::code`]
    /// which is the *next* bytecode instruction to be executed.
    ip: usize,
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
    mut run: impl FnMut(&mut Vm) -> Result<Value>
) -> Result<Value> {
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
            // This is just a placeholder, the instruction pointer will be overridden once a function is called.
            ip: 0,
        }
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

        let mut frames = self.call_stack.stack.iter()
            .rev()
            .filter(|frame| !matches!(frame.kind, FrameKind::Temp { .. }));

        if let Some(first) = frames.next() {
            let mut address = match first.kind {
                FrameKind::UserFunction => Some(self.ip),
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
