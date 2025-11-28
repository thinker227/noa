use crate::value::Value;
use crate::vm::{Vm, Result};

pub mod functions;

/// A function implemented natively within the runtime.
#[derive(Debug, Clone)]
pub struct NativeFunction {
    /// The name of the function.
    pub name: String,
    /// Function pointer to the native function.
    /// The function is given a mutable reference to the VM to access things like the heap
    /// and invoking other functions, as well as the *raw* arguments to the passed.
    pub function: NativeFn,
}

pub type NativeFn = fn(&mut Vm, Vec<Value>) -> Result<Value>;
