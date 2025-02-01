use crate::exception::Exception;
use crate::value::{Closure, Value};
use crate::vm::Vm;

/// A function implemented natively within the runtime.
/// 
/// Alias for a function pointer.
/// The function is given a slice of values as the arguments to the native function,
/// and returns a `Box<dyn NativeCall>` which should be an initialized state machine
/// for the execution of the function.
pub type NativeFunction = fn(&[Value]) -> Box<dyn NativeCall>;

/// Controls the behavior of the virtual machine after calling [`NativeCall::execute`].
pub enum NativeCallControlFlow {
    /// The vm will invoke a closure.
    /// After the closure has finished execution,
    /// control will be returned back to the native function.
    Call(Closure),
    /// The execution of the native function will terminate.
    /// The value returned will be used as the return value of the function.
    Return(Value),
}

/// A call to a native function.
/// 
/// This trait is meant to allow native functions to be structured as state machines,
/// where calling [`NativeCall::execute`] will continue execution of the function and advance the state machine.
/// This is to allow native functions to call into user code using [`NativeCallControlFlow`].
pub trait NativeCall {
    /// Executes a 'step' of the native function.
    /// If the native function is implemented as a state machine, this should advance it.
    fn execute(&mut self, vm: &mut Vm) -> Result<NativeCallControlFlow, Exception>;
}
