use crate::exception::Exception;
use crate::value::Value;
use crate::vm::Vm;

/// A function implemented natively within the runtime.
/// 
/// Alias for a function pointer.
/// The function is given a mutable reference to the VM to access things like the heap
/// and invoking other functions, as well as the *raw* arguments to the passed.
pub type NativeFunction = fn(&mut Vm, Vec<Value>) -> Result<Value, Exception>;
