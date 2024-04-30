use super::value::coercion::CoercionError;

/// The kind of a runtime exception.
#[derive(Debug, PartialEq, Eq)]
pub enum ExceptionKind {
    /// No stack frame exists on the call stack.
    NoStackFrame,
    /// Attempted to execute code past the bounds of a function's code.
    FunctionOverrun,
    /// The stack or call stack overflowed.
    StackOverflow,
    /// The stack or call stack underflowed.
    StackUnderflow,
    /// A value coercion error occurred.
    CoercionError(CoercionError),
    /// A function with an invalid function ID was referenced.
    InvalidFunction,
    /// Attempted to execute an unsupported operation.
    Unsupported,
}

/// A runtime exception.
#[derive(Debug, PartialEq, Eq)]
pub struct Exception {
    kind: ExceptionKind,
    // Todo: call stack trace, debug info, etc.
}

impl Exception {
    /// Constructs a new exception.
    pub fn new(kind: ExceptionKind) -> Self {
        Self {
            kind
        }
    }

    /// Gets the kind of the exception.
    pub fn kind(&self) -> &ExceptionKind {
        &self.kind
    }
}
