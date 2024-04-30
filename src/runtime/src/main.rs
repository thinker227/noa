#![allow(dead_code, unused_variables)]

use std::{fs, io, path::Path};

use clap::Parser;
use cli::Args;

use ark::Ark;
use runtime::{exception::{Exception, ExceptionKind}, virtual_machine::VM};

mod cli;
mod byte_utility;
mod ark;
mod runtime;

fn main() {
    let args = Args::parse();

    let ark_bytes = if let Some(path) = &args.bytecode_file_path {
        read_ark_from_file_path(path)
    } else {
        eprintln!(".ark file path was not provided");
        std::process::exit(1);
    };

    match ark_bytes {
        Ok(ark_bytes) => {
            let ark = match Ark::from_bytes(ark_bytes.as_slice()) {
                Ok(ark) => ark,
                Err(_) => {
                    eprintln!("Error reading .ark file.");
                    std::process::exit(1);
                }
            };

            execute(ark);
        }
        Err(e) => match e {
            ArkReadError::IoError(e) => {
                eprintln!("{e}");
                std::process::exit(1);
            }
        }
    }
}

fn execute(ark: Ark) -> () {
    let mut vm = VM::new(ark, 2_000, 10_000);
    let result = vm.execute_main();

    match result {
        Ok(ret_value) => {
            println!("{ret_value}");

            std::process::exit(0);
        },
        Err(e) => {
            let message = exception_message(&e);

            eprintln!("An exception occurred!");
            eprintln!("  {message}");

            std::process::exit(1);
        }
    }
}

fn exception_message(e: &Exception) -> String {
    match e.kind() {
        ExceptionKind::NoStackFrame => "There was no stack frame on the call stack.".into(),
        ExceptionKind::FunctionOverrun => "Attempted to execute instructions past the function's bounds. (forgot a ret (0x04) instruction?) (jumped out of bounds?)".into(),
        ExceptionKind::StackOverflow => "Stack overflow. (infinite recursion?)".into(),
        ExceptionKind::StackUnderflow => "Stack underflow. (pushed too little onto the stack?)".into(),
        ExceptionKind::CoercionError(c) => {
            let c_msg = match *c {
                runtime::value::coercion::CoercionError::ToFunction => "coercion to a function",
                runtime::value::coercion::CoercionError::ToNil => "coercion to ()",
                runtime::value::coercion::CoercionError::FunctionToNumber => "coercion from function to number",
            };
            format!("Coercion error: {c_msg}")
        },
        ExceptionKind::InvalidFunction => "Attempted to call an invalid function. (does the function exists?)".into(),
        ExceptionKind::Unsupported => "Attempted to execute an unsupported operation.".into(),
    }
}

#[derive(Debug)]
enum ArkReadError {
    IoError(io::Error)
}

fn read_ark_from_file_path(path: &Path) -> Result<Vec<u8>, ArkReadError> {
    fs::read(path).map_err(|e| ArkReadError::IoError(e))
}
