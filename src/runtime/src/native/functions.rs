use std::io::Write;

use crate::exception::Exception;
use crate::heap::HeapValue;
use crate::vm::{Vm, Result};
use crate::value::Value;

use super::NativeFunction;

/// Gets a vector of native functions.
pub fn get_functions() -> Vec<NativeFunction> {
    vec![
        print,
        get_input
    ]
}

fn print(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let (value, append_newline) = match args[..] {
        [] => (String::from(""), true),

        [val] => (vm.to_string(val)?, true),

        [val, append_newline, ..] => (
            vm.to_string(val)?,
            vm.coerce_to_bool(append_newline)?
        )
    };

    let mut stdout = std::io::stdout();

    let stdout_exception = |_| {
        vm.exception(Exception::Custom("failed to write to stdout".into()))
    };

    write!(stdout, "{value}").map_err(stdout_exception)?;
    
    if append_newline {
        writeln!(stdout).map_err(stdout_exception)?;
    }

    stdout.flush().map_err(stdout_exception)?;

    Ok(Value::Nil)
}

fn get_input(vm: &mut Vm, _: Vec<Value>) -> Result<Value> {
    let stdin = std::io::stdin();

    let mut buf = String::new();
    let _ = stdin.read_line(&mut buf)
        .map_err(|_| vm.exception(
            Exception::Custom("failed to read from stdio".into())
        ))?;
    
    // Pop the trailing newline character
    buf.pop();

    let address = vm.alloc_heap_value(HeapValue::String(buf))?;

    Ok(Value::Object(address))
}
