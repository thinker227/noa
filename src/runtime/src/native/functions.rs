use std::collections::HashMap;
use std::fs;
use std::io::Write;
use std::iter;

use crate::exception::Exception;
use crate::vm::{Vm, Result};
use crate::value::{Closure, List, Value};

use super::NativeFunction;

/// Gets a vector of native functions.
pub fn get_functions() -> HashMap<u32, NativeFunction> {
    let functions: [(u32, NativeFunction); _] = [
        // Console IO
        (0x0, print),
        (0x1, get_input),

        // File IO
        (0x80, read_file),
        (0x81, write_file),

        // Strings
        (0x100, to_string),
        
        // Lists
        (0x180, push),
        (0x181, pop),
        (0x182, append),
        (0x183, concat),
        (0x184, slice),
        (0x185, map),
        (0x186, flat_map),
        (0x187, filter),
        (0x188, reduce),
        (0x189, reverse),
        (0x18A, any),
        (0x18B, all),
        (0x18C, find),
        (0x18D, length),
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

    Ok(().into())
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
        Err(_) => Ok(().into()),
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

fn push(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(list), _), value) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [list, value, ..] => (
            vm.coerce_to_list_mut(list)?,
            value
        )
    };

    list.push(value);

    Ok(().into())
}

fn pop(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let (list, _) = match args[..] {
        [] => return Err(vm.exception(
            Exception::BadArity { expected: 1, or_more: false, actual: args.len() as u32 }
        )),

        [list, ..] => vm.coerce_to_list_mut(list)?
    };

    let val = match list.0.pop() {
        Some(x) => x,
        None => ().into()
    };

    Ok(val)
}

fn append(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((list, _), value) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [list, value, ..] => (
            vm.coerce_to_list(list)?,
            value
        )
    };

    let mut vec = list.0.clone();
    vec.push(value);

    vm.alloc_list(vec)
}

fn concat(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((a, _), (b, _)) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [a, b, ..] => (
            vm.coerce_to_list(a)?,
            vm.coerce_to_list(b)?
        )
    };

    let vec = a.0.iter().copied()
        .chain(b.0.iter().copied())
        .collect::<Vec<_>>();

    vm.alloc_list(vec)
}

fn slice(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(list), _), start, end) = match args[..] {
        [] | [_] | [_, _] => return Err(vm.exception(
            Exception::BadArity { expected: 3, or_more: false, actual: args.len() as u32 }
        )),

        [list, start, end, ..] => (
            vm.coerce_to_list(list)?,
            vm.coerce_to_number(start)?,
            vm.coerce_to_number(end)?
        )
    };
    
    let start = vm.to_integer(start)?;
    let end = vm.to_integer(end)?;
    let start = start.clamp(0, list.len() as i64);
    let end = end.clamp(0, list.len() as i64);

    let length = end - start;

    if length <= 0 {
        return vm.alloc_list(iter::empty());
    }

    let slice = list[start as usize..end as usize].to_vec();

    vm.alloc_list(slice)
}

fn map(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(source), _), map) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [source, map, ..] => (
            vm.coerce_to_list(source)?,
            vm.coerce_to_function(map)?
        )
    };

    let mut result = source.clone();
    for (i, x) in result.iter_mut().enumerate() {
        let mapped = vm.call_run(map, &[*x, i.into()])?;
        *x = mapped;
    }

    vm.alloc_list(result)
}

fn flat_map(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(source), _), map) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [source, map, ..] => (
            vm.coerce_to_list(source)?,
            vm.coerce_to_function(map)?
        )
    };

    let mut result = Vec::new();
    for (i, x) in source.clone().iter().enumerate() {
        let mapped = vm.call_run(map, &[*x, i.into()])?;
        let (List(mapped), _) = vm.coerce_to_list(mapped)?;

        result.extend(mapped.iter().copied());
    }

    vm.alloc_list(result)
}

fn filter(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(source), _), filter) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [source, filter, ..] => (
            vm.coerce_to_list(source)?,
            vm.coerce_to_function(filter)?
        )
    };

    // Wish we could use `Vec::retain` here, but we have to be able to return exceptions, so we can't.
    let mut result = Vec::new();
    for (i, x) in source.clone().iter().enumerate() {
        let retain = vm.call_run(filter, &[*x, i.into()])?;
        let retain = vm.coerce_to_bool(retain)?;

        if retain {
            result.push(*x);
        }
    }

    vm.alloc_list(result)
}

fn reduce(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(source), _), seed, reduce) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: true, actual: args.len() as u32 }
        )),

        [source, reduce] => (
            vm.coerce_to_list(source)?,
            None,
            vm.coerce_to_function(reduce)?
        ),

        [source, seed, reduce, ..] => (
            vm.coerce_to_list(source)?,
            Some(seed),
            vm.coerce_to_function(reduce)?
        )
    };

    let (seed, elements) = match seed {
        Some(x) => (x, &source.clone()[0..]),
        None => match source.first() {
            Some(x) => (*x, &source.clone()[1..]),
            None => return Err(vm.exception(
                Exception::Custom(String::from("expected list to contain at least 1 element since no seed was passed to `reduce`"))
            ))
        }
    };

    let mut result = seed;
    for x in elements {
        result = vm.call_run(reduce, &[result, *x])?;
    }

    Ok(result)
}

fn reverse(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let (List(source), _) = match args[..] {
        [] => return Err(vm.exception(
            Exception::BadArity { expected: 1, or_more: false, actual: args.len() as u32 }
        )),

        [source, ..] => vm.coerce_to_list(source)?,
    };

    let mut result = source.clone();
    result.reverse();

    vm.alloc_list(result)
}

fn any(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(source), _), predicate) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [source, predicate, ..] => (
            vm.coerce_to_list(source)?,
            vm.coerce_to_function(predicate)?
        )
    };

    if source.is_empty() {
        return Ok(().into());
    }

    for x in source.clone() {
        let result = vm.call_run(predicate, &[x])?;
        let result = vm.coerce_to_bool(result)?;

        if result {
            return Ok(true.into());
        }
    }

    Ok(false.into())
}

fn all(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(source), _), predicate) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: false, actual: args.len() as u32 }
        )),

        [source, predicate, ..] => (
            vm.coerce_to_list(source)?,
            vm.coerce_to_function(predicate)?
        )
    };

    if source.is_empty() {
        return Ok(().into());
    }

    for x in source.clone() {
        let result = vm.call_run(predicate, &[x])?;
        let result = vm.coerce_to_bool(result)?;

        if !result {
            return Ok(false.into());
        }
    }

    Ok(true.into())
}

fn find(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let ((List(source), _), predicate, from_end) = match args[..] {
        [] | [_] => return Err(vm.exception(
            Exception::BadArity { expected: 2, or_more: true, actual: args.len() as u32 }
        )),

        [source, predicate] => (
            vm.coerce_to_list(source)?,
            vm.coerce_to_function(predicate)?,
            false
        ),

        [source, predicate, from_end, ..] => (
            vm.coerce_to_list(source)?,
            vm.coerce_to_function(predicate)?,
            vm.coerce_to_bool(from_end)?
        )
    };

    return if from_end {
        iter(vm, predicate, source.clone().into_iter().rev())
    } else {
        iter(vm, predicate, source.clone().into_iter())
    };

    fn iter(vm: &mut Vm, predicate: Closure, xs: impl Iterator<Item = Value>) -> Result<Value> {
        for x in xs {
            let result = vm.call_run(predicate, &[x])?;
            let result = vm.coerce_to_bool(result)?;

            if result {
                return Ok(x);
            }
        }

        Ok(().into())
    }
}

fn length(vm: &mut Vm, args: Vec<Value>) -> Result<Value> {
    let (List(list), _) = match args[..] {
        [] => return Err(vm.exception(
            Exception::BadArity { expected: 1, or_more: false, actual: args.len() as u32 }
        )),

        [list, ..] => vm.coerce_to_list(list)?,
    };

    Ok(list.len().into())
}
