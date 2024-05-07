use std::fmt::Display;

use super::value::coercion::CoercionError;

/// A runtime exception.
#[derive(Debug, PartialEq, Eq)]
pub struct Exception {
    kind: ExceptionKind,
    // Todo: call stack trace, debug info, etc.
}

/// The kind of a runtime exception.
#[derive(Debug, PartialEq, Eq)]
pub enum ExceptionKind {
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
    /// Constructs a new code exception.
    pub fn code(ex: CodeException) -> Self {
        Self {
            kind: ExceptionKind::Code(ex)
        }
    }

    /// Constructs a new virtual machine exception.
    pub fn vm(ex: VMException) -> Self {
        Self {
            kind: ExceptionKind::VM(ex)
        }
    }

    /// Gets the kind of the exception.
    pub fn kind(&self) -> &ExceptionKind {
        &self.kind
    }
}

impl Display for ExceptionKind {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            ExceptionKind::Code(e) => write!(f, "{e}"),
            ExceptionKind::VM(e) => write!(f, "{e}"),
        }
    }
}

impl Display for CodeException {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            CodeException::CoercionError(e) => write!(f, "conversion error: {e}"),
            CodeException::DivisionBy0 => write!(f, "division by 0"),
        }
    }
}

impl Display for VMException {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            VMException::NoStackFrame => write!(f, "there are no stack frames on the call stack"),
            VMException::CallStackOverflow => write!(f, "call stack overflowed"),
            VMException::FunctionOverrun => write!(f, "attempted to execute code outside the bounds of the function's code"),
            VMException::StackOverflow => write!(f, "stack overflowed"),
            VMException::StackUnderflow => write!(f, "stack underflow"),
            VMException::InvalidFunction => write!(f, "attempted to reference a function with an invalid ID"),
            VMException::InvalidVariable => write!(f, "attempted to reference a variable with an invalid ID"),
            VMException::InvalidString => write!(f, "attempted to reference a string with an invalid index"),
            VMException::Unsupported => write!(f, "attempted to execute an unsupported operation"),
        }
    }
}
