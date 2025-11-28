use std::collections::HashMap;

use debugger::Debugger;
use frame::{Frame, FrameKind};
use polonius_the_crab::{polonius, polonius_return};
use stack::Stack;

use crate::ark::Function;
use crate::exception::{Exception, FormattedException, TraceFrame};
use crate::native::{functions, NativeFunction};
use crate::heap::{Heap, HeapAddress, HeapAllocError, HeapGetError, HeapValue};
use crate::value::{Field, List, Object, Value};

pub mod frame;
pub mod stack;
pub mod debugger;
mod interpret;
mod value_ops;

/// The result of a VM operation.
pub type Result<T> = std::result::Result<T, FormattedException>;

pub trait Input {
    fn read(&mut self, buf: &mut Vec<u8>) -> std::result::Result<(), Exception>;
}

pub trait Output {
    fn write(&mut self, bytes: &[u8]) -> std::result::Result<(), Exception>;
}

/// Constants for a single execution of the virtual machine.
pub struct VmConsts {
    /// User functions.
    pub functions: Vec<Function>,
    /// Native functions.
    pub native_functions: HashMap<u32, NativeFunction>,
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
    /// Input stream.
    input: Box<dyn Input>,
    /// Output stream.
    output: Box<dyn Output>,
    /// The debugger interface.
    debugger: Option<Box<dyn Debugger>>,
}

impl Vm {
    #[allow(clippy::too_many_arguments)]
    /// Creates a new [`Vm`].
    pub fn new(
        functions: Vec<Function>,
        strings: Vec<String>,
        code: Vec<u8>,
        stack_size: usize,
        call_stack_size: usize,
        heap_size: usize,
        input: Box<dyn Input>,
        output: Box<dyn Output>,
        debugger: Option<Box<dyn Debugger>>
    ) -> Self {
        Self {
            consts: VmConsts {
                functions,
                native_functions: functions::get_functions(),
                strings,
                code
            },
            stack: Stack::new(stack_size),
            heap: Heap::new(heap_size),
            call_stack: Vec::with_capacity(call_stack_size),
            // This is just a placeholder, the instruction pointer will be overridden once a function is called.
            ip: 0,
            trace_ip: 0,
            input,
            output,
            debugger
        }
    }

    /// Gets the heap.
    pub fn heap(&mut self) -> &mut Heap {
        &mut self.heap
    }

    /// Gets the input stream.
    pub fn input(&mut self) -> &mut dyn Input  {
        &mut *self.input 
    }

    /// Gets the output stream.
    pub fn output(&mut self) -> &mut dyn Output {
        &mut *self.output
    }
    
    /// Gets the debugger interface.
    pub fn debugger(&mut self) -> &mut Option<Box<dyn Debugger>> {
        &mut self.debugger
    }

    /// Gets a value at a specified address on the heap.
    pub fn get_heap_value(&self, address: HeapAddress) -> Result<&HeapValue> {
        self.heap.get(address)
            .map_err(|e| {
                let ex = match e {
                    HeapGetError::OutOfBounds => Exception::OutOfBoundsHeapAddress,
                    HeapGetError::SlotFreed => Exception::FreedHeapAddress,
                };
                self.exception(ex)
            })
    }

    /// Gets a value at a specified address on the heap.
    fn get_heap_value_mut(&mut self, address: HeapAddress) -> Result<&mut HeapValue> {
        // This single function requires an entire crate just to get around borrow checker issues.
        // The flow of this function is to get the value on the heap mutably and return it as Ok
        // and otherwise match on the error and construct an exception using it.
        // However, the current borrow checker isn't smart enough to know that the borrow of self from self.heap
        // is not longer relevant after the match, so we need to use Polonius which emulates the behavior
        // of the newer Polonius borrow checker to get around this issue.

        let mut this = self;
        
        let ex = polonius!(|this| -> Result<&'polonius mut HeapValue> {
            match this.heap.get_mut(address) {
                Ok(val) => polonius_return!(Ok(val)),
                Err(e) => match e {
                    HeapGetError::OutOfBounds => Exception::OutOfBoundsHeapAddress,
                    HeapGetError::SlotFreed => Exception::FreedHeapAddress,
                }
            }
        });

        Err(this.exception(ex))
    }

    /// Allocates a value on the heap.
    pub fn heap_alloc(&mut self, value: HeapValue) -> Result<HeapAddress> {
        match self.heap.alloc(value) {
            Ok(x) => Ok(x),
            Err(HeapAllocError::OutOfMemory(value)) => {
                // We're out of heap memory, so do a run of garbage collection.
                // This is an extremely naïve approach to garbage collection,
                // although we don't really need much more since we're not really
                // in the business of performance anyway.
                let roots = self.stack.iter().copied();
                self.heap.collect(roots);
                
                // If we're still out of memory after doing a run of garbage collection,
                // then we're *truly* out of memory.
                self.heap.alloc(value)
                    .map_err(|_| self.exception(Exception::OutOfMemory))
            },
        }
        // self.heap.alloc(value)
        //     .map_err(|_| self.exception(Exception::OutOfMemory))
    }

    /// Allocates a string on the heap.
    pub fn alloc_string(&mut self, string: String) -> Result<Value> {
        self.heap_alloc(HeapValue::String(string))
            .map(Value::Object)
    }
    
    /// Allocates a list on the heap.
    pub fn alloc_list(&mut self, values: impl IntoIterator<Item = Value>) -> Result<Value> {
        let values = values.into_iter().collect();

        self.heap_alloc(HeapValue::List(List(values)))
            .map(Value::Object)
    }

    /// Allocates an object on the heap.
    pub fn alloc_object(&mut self, fields: impl IntoIterator<Item = (String, Field)>, dynamic: bool) -> Result<Value> {
        let object = Object {
            fields: fields.into_iter().collect(),
            dynamic
        };

        self.heap_alloc(HeapValue::Object(object))
            .map(Value::Object)
    }

    /// Formats an [`Exception`] into a [`FormattedException`].
    pub fn exception(&self, exception: Exception) -> FormattedException {
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
