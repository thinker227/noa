use thiserror::Error;

use crate::vm::frame::{Frame, FrameReturn};
use crate::ark::FuncId;

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
}

/// An [`Exception`] formatted with a stack trace.
pub struct FormattedException {
    pub exception: Exception,
    pub stack_trace: Vec<TraceFrame>,
}

/// A frame in a stack trace.
pub struct TraceFrame {
    pub function: FuncId,
    pub address: Option<usize>,
}

impl From<&Frame> for TraceFrame {
    fn from(value: &Frame) -> Self {
        Self {
            function: value.function,
            address: match value.ret {
                FrameReturn::User(adr) => Some(adr),
                _ => None
            }
        }
    }
}
