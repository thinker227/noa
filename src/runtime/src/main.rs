#![allow(dead_code, unused_variables)]

use std::{fs, io};
use std::fmt::Write as _;
use std::path::Path;
use std::process::{ExitCode, Termination};

use clap::Parser;
use cli::Args;

use ark::Ark;
use vm::VM;
use runtime::exception::{Exception, StackTraceAddress};

mod cli;
mod utility;
mod ark;
mod runtime;
mod vm;

fn main() -> Exit {
    let args = Args::parse();

    let ark_bytes = if let Some(path) = &args.bytecode_file_path {
        read_ark_from_file_path(path)
    } else {
        return Exit::Failure(".ark file path was not provided".into());
    };

    match ark_bytes {
        Ok(ark_bytes) => {
            let ark = match Ark::from_bytes(ark_bytes.as_slice()) {
                Ok(ark) => ark,
                Err(_) => {
                    return Exit::Failure("Error reading .ark file.".into());
                }
            };

            let result = execute(ark, args.print_return_value);

            match result {
                Ok(exit_code) => Exit::Code(exit_code as u8),
                Err(message) => Exit::Failure(message),
            }
        }
        Err(e) => match e {
            ArkReadError::IoError(e) => {
                Exit::Failure(e.to_string())
            }
        }
    }
}

fn execute(ark: Ark, print_return_value: bool) -> Result<i32, String> {
    let mut vm = VM::new(ark, 2_000, 10_000);

    let result = vm.execute_main();

    match result {
        Ok(ret_value) => {
            let main = vm.functions().get(&vm.main()).unwrap();
            let main_name = vm.get_string_or_fallback(main.name_index(), "?");
            let exit_code = ret_value.exit_code();
            
            if print_return_value {
                println!("{ret_value}");
            }

            Ok(exit_code)
        },
        Err(e) => {
            // I don't think formatting an exception can ever fail.
            let formatted = format_exception(&e, &vm).unwrap();
            Err(formatted)
        }
    }
}

fn format_exception(e: &Exception, vm: &VM) -> Result<String, std::fmt::Error> {
    let mut str = String::new();

    writeln!(&mut str, "An exception occurred!")?;
    writeln!(&mut str, "  {}", e.data().to_string())?;
    writeln!(&mut str, "")?;
    writeln!(&mut str, "  Stack trace:")?;
    for f in e.stack_trace() {
        let function = vm.functions().get(&f.function).unwrap();
        let func_name = vm.get_string(function.name_index()).unwrap();
        let address = match f.address {
            StackTraceAddress::Explicit(x) => format!("{x:x}"),
            StackTraceAddress::Implicit => "<runtime code>".into(),
        };

        writeln!(&mut str, "    at address 0x{0} (in {1})", address, func_name)?;
    }

    Ok(str)
}

#[derive(Debug)]
enum ArkReadError {
    IoError(io::Error)
}

fn read_ark_from_file_path(path: &Path) -> Result<Vec<u8>, ArkReadError> {
    fs::read(path).map_err(|e| ArkReadError::IoError(e))
}

enum Exit {
    Code(u8),
    Failure(String),
}

impl Termination for Exit {
    fn report(self) -> ExitCode {
        match self {
            Self::Code(x) => ExitCode::from(x),
            Self::Failure(s) => {
                eprintln!("{s}");
                ExitCode::FAILURE
            }
        }
    }
}
