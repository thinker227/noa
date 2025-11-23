use thiserror::Error;

/// A runtime exception.
#[derive(Debug, Error)]
pub enum Exception {
    #[error("stack overflow")]
    StackOverflow,

    #[error("stack underflow")]
    StackUnderflow,

    #[error("execution continued past the bounds of the current function")]
    Overrun,

    #[error("unknown opcode `{0}`")]
    UnknownOpcode(u8),

    #[error("invalid function `{0}`")]
    InvalidUserFunction(u32),

    #[error("invalid native function `{0}`")]
    InvalidNativeFunction(u32),

    #[error("call stack overflow")]
    CallStackOverflow,

    #[error("function exhausted the call stack without returning")]
    NoReturn,

    #[error("tried to reference an out-of-bounds heap address")]
    OutOfBoundsHeapAddress,

    #[error("tried to reference a heap address which memory has been freed")]
    FreedHeapAddress,

    #[error("cannot coerce {0} to into {1}")]
    CoercionError(String, String),

    #[error("invalid variable `{0}`")]
    InvalidVariable(usize),

    #[error("invalid string `{0}`")]
    InvalidString(usize),

    #[error("out of memory")]
    OutOfMemory,

    #[error("field \"{0}\" does not exist")]
    MissingField(String),

    #[error("cannot write to immutable field \"{0}\"")]
    WriteToImmutableField(String),

    #[error("cannot index into list using index `{0}`")]
    InvalidIndex(f64),

    #[error("index `{0}` is out of bounds of the list, which has a length of `{1}`")]
    OutOfBoundsIndex(f64, usize),

    #[error("the string `{0}` is not valid UTF-8")]
    NonUtf8(String),

    #[error("expected {}{} arguments but got {}", expected, if *or_more { " or more" } else { "" }, actual)]
    BadArity {
        expected: u32,
        or_more: bool,
        actual: u32,
    },

    #[error("expected parameter {param} to function {function} to be of type {expected} but was {actual}")]
    BadArgumentType {
        param: String,
        function: String,
        expected: String,
        actual: String,
    },

    #[error("{0}")]
    Custom(String),
}

/// An [`Exception`] formatted with a stack trace.
#[derive(Debug)]
pub struct FormattedException {
    pub exception: Exception,
    pub stack_trace: Vec<TraceFrame>,
}

/// A frame in a stack trace.
#[derive(Debug)]
pub struct TraceFrame {
    pub function: String,
    pub address: Option<usize>,
}
