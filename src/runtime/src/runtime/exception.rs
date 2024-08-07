use std::fmt::Display;

use crate::ark::opcode::FuncId;
use super::value::coercion::CoercionError;

#[derive(Debug, PartialEq, Eq)]
pub struct StackTraceFrame {
    pub function: FuncId,
    pub address: StackTraceAddress,
}

#[derive(Debug, PartialEq, Eq)]
pub enum StackTraceAddress {
    Explicit(usize),
    Implicit
}

/// A runtime exception.
#[derive(Debug, PartialEq, Eq)]
pub struct Exception {
    data: ExceptionData,
    stack_trace: Vec<StackTraceFrame>,
    // Todo: call stack trace, debug info, etc.
}

/// The kind of a runtime exception.
#[derive(Debug, PartialEq, Eq)]
pub enum ExceptionData {
    Code(CodeException),
    VM(VMException),
}

/// An exception caused by code.
#[derive(Debug, PartialEq, Eq)]
pub enum CodeException {
    /// A value coercion error occurred.
    CoercionError(CoercionError),
    /// Division by 0.
    DivisionBy0,
    /// Integer overflow.
    IntegerOverflow,
}

/// An irrecoverable virtual machine exception.
#[derive(Debug, PartialEq, Eq)]
pub enum VMException {
    /// No stack frame exists on the call stack.
    NoStackFrame,
    /// The call stack overflowed.
    CallStackOverflow,
    /// Attempted to execute code past the bounds of a function's code.
    FunctionOverrun,
    /// Attempted to read a malformed opcode.
    MalformedOpcode,
    /// Attempted to execute an invalid opcode.
    InvalidOpcode,
    /// The stack overflowed.
    StackOverflow,
    /// The stack underflowed.
    StackUnderflow,
    /// A function with an invalid function ID was referenced.
    InvalidFunction,
    /// A variable with an invalid index was referenced.
    InvalidVariable,
    /// A string with an invalid index was referenced.
    InvalidString,
    /// Attempted to execute an unsupported operation.
    Unsupported,
}

impl Exception {
    /// Constructs a new exception.
    pub fn new(data: ExceptionData, stack_trace: Vec<StackTraceFrame>) -> Self {
        Self {
            data,
            stack_trace,
        }
    }

    /// Constructs a new code exception.
    pub fn code(ex: CodeException, stack_trace: Vec<StackTraceFrame>) -> Self {
        Self {
            data: ExceptionData::Code(ex),
            stack_trace,
        }
    }

    /// Constructs a new virtual machine exception.
    pub fn vm(ex: VMException, stack_trace: Vec<StackTraceFrame>) -> Self {
        Self {
            data: ExceptionData::VM(ex),
            stack_trace,
        }
    }

    /// Gets the kind of the exception.
    pub fn data(&self) -> &ExceptionData {
        &self.data
    }

    /// Gets the stack trace.
    pub fn stack_trace(&self) -> &Vec<StackTraceFrame> {
        &self.stack_trace
    }
}

impl Display for ExceptionData {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            ExceptionData::Code(e) => write!(f, "{e}"),
            ExceptionData::VM(e) => write!(f, "{e}"),
        }
    }
}

impl Display for CodeException {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            CodeException::CoercionError(e) => write!(f, "conversion error: {e}"),
            CodeException::DivisionBy0 => write!(f, "division by 0"),
            CodeException::IntegerOverflow => write!(f, "integer overflow"),
        }
    }
}

impl Display for VMException {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            VMException::NoStackFrame => write!(f, "there are no stack frames on the call stack"),
            VMException::CallStackOverflow => write!(f, "call stack overflowed"),
            VMException::FunctionOverrun => write!(f, "attempted to execute code outside the bounds of the function's code"),
            VMException::MalformedOpcode => write!(f, "attempted to read a malformed opcode"),
            VMException::InvalidOpcode => write!(f, "attempted to execute an invalid opcode"),
            VMException::StackOverflow => write!(f, "stack overflowed"),
            VMException::StackUnderflow => write!(f, "stack underflow"),
            VMException::InvalidFunction => write!(f, "attempted to reference a function with an invalid ID"),
            VMException::InvalidVariable => write!(f, "attempted to reference a variable with an invalid ID"),
            VMException::InvalidString => write!(f, "attempted to reference a string with an invalid index"),
            VMException::Unsupported => write!(f, "attempted to execute an unsupported operation"),
        }
    }
}
