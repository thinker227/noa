use std::collections::HashMap;
use std::fs;
use std::io::Write;

use crate::exception::Exception;
use crate::vm::{Vm, Result};
use crate::value::Value;

use super::NativeFunction;

/// Gets a vector of native functions.
pub fn get_functions() -> HashMap<u32, NativeFunction> {
    let functions: [(u32, NativeFunction); _] = [
        (0x0, print),
        (0x1, get_input),
        (0x80, read_file),
        (0x81, write_file),
        (0x100, to_string),
        // (0x180, push),
        // (0x181, pop),
        // (0x182, append),
        // (0x183, concat),
        // (0x184, slice),
        // (0x185, map),
        // (0x186, flatMap),
        // (0x187, filter),
        // (0x188, reduce),
        // (0x189, reverse),
        // (0x18A, any),
        // (0x18B, all),
        // (0x18C, find),
    ];

    functions.into_iter().collect()
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

    vm.alloc_string(buf)
}

fn read_file(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let path = match args[..] {
        [] => return Err(vm.exception(
            Exception::BadArity { expected: 1, or_more: false, actual: 0 }
        )),

        [path, ..] => vm.to_string(path)?,
    };

    // let contents = fs::read_to_string(path.clone())
    //     .map_err(|e| vm.exception(
    //         Exception::Custom(match e.kind() {
    //             std::io::ErrorKind::NotFound => format!("could not find file {path}"),
    //             std::io::ErrorKind::PermissionDenied => format!("permission to access {path} was defined"),
    //             std::io::ErrorKind::IsADirectory => format!("{path} is a directory, expected a file"),
    //             std::io::ErrorKind::ReadOnlyFilesystem => format!("file system of {path} is read-only"),
    //             std::io::ErrorKind::InvalidFilename => format!("invalid file name of {path}"),
    //             std::io::ErrorKind::OutOfMemory => format!("failed to read file {path}, out of memory"),
    //             _ => format!("failed to read file {path}"),
    //         })
    //     ))?;

    match fs::read_to_string(path) {
        Ok(content) => vm.alloc_string(content),
        Err(_) => Ok(Value::Nil),
    }
}

fn write_file(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let arg_count = args.len() as u32;
    let (path, content) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: arg_count }
        )),

        [path, content, ..] => (vm.to_string(path)?, vm.to_string(content)?),
    };

    // fs::write(path.clone(), content)
    //     .map_err(|e| vm.exception(
    //         Exception::Custom(match e.kind() {
    //             std::io::ErrorKind::NotFound => format!("could not find file {path}"),
    //             std::io::ErrorKind::PermissionDenied => format!("permission to access {path} was defined"),
    //             std::io::ErrorKind::IsADirectory => format!("{path} is a directory, expected a file"),
    //             std::io::ErrorKind::ReadOnlyFilesystem => format!("file system of {path} is read-only"),
    //             std::io::ErrorKind::InvalidFilename => format!("invalid file name of {path}"),
    //             std::io::ErrorKind::OutOfMemory => format!("failed to write to file {path}, out of memory"),
    //             _ => format!("failed to write to file {path}"),
    //         })
    //     ))?;

    match fs::write(path, content) {
        Ok(()) => Ok(Value::Bool(true)),
        Err(_) => Ok(Value::Bool(false))
    }
}

fn to_string(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    match args[..] {
        [] => {
            vm.alloc_string(String::from(""))
        },

        [val, ..] => {
            let str = vm.to_string(val)?;
            vm.alloc_string(str)
        }
    }
}
