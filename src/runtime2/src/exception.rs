use crate::vm::frame::{Frame, FrameReturn};
use crate::ark::FuncId;

/// A runtime exception.
pub enum Exception {

}

/// An [`Exception`] formatted with a stack trace.
pub struct FormattedException {
    exception: Exception,
    stack_trace: Vec<TraceFrame>,
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
