#![allow(dead_code, unused_variables)]

use std::{fs, io};
use std::path::Path;
use std::process::ExitCode;

use clap::Parser;
use cli::Args;

use ark::Ark;
use vm::VM;
use runtime::exception::StackTraceAddress;

mod cli;
mod utility;
mod ark;
mod runtime;
mod vm;

fn main() -> ExitCode {
    let args = Args::parse();

    let ark_bytes = if let Some(path) = &args.bytecode_file_path {
        read_ark_from_file_path(path)
    } else {
        eprintln!(".ark file path was not provided");
        return ExitCode::FAILURE;
    };

    match ark_bytes {
        Ok(ark_bytes) => {
            let ark = match Ark::from_bytes(ark_bytes.as_slice()) {
                Ok(ark) => ark,
                Err(_) => {
                    eprintln!("Error reading .ark file.");
                    return ExitCode::FAILURE;
                }
            };

            let result = execute(ark, args.print_return_value);

            result.map(|x| ExitCode::from(x as u8))
                .unwrap_or(ExitCode::FAILURE)
        }
        Err(e) => match e {
            ArkReadError::IoError(e) => {
                eprintln!("{e}");
                ExitCode::FAILURE
            }
        }
    }
}

fn execute(ark: Ark, print_return_value: bool) -> Result<i32, ()> {
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
            eprintln!("An exception occurred!");
            eprintln!("  {}", e.data().to_string());
            eprintln!();
            eprintln!("  Stack trace:");
            for f in e.stack_trace() {
                let function = vm.functions().get(&f.function).unwrap();
                let func_name = vm.get_string(function.name_index()).unwrap();
                let address = match f.address {
                    StackTraceAddress::Explicit(x) => format!("{x:x}"),
                    StackTraceAddress::Implicit => "<runtime code>".into(),
                };

                eprintln!("    at address 0x{0} in {1}", address, func_name);
            }

            Err(())
        }
    }
}

#[derive(Debug)]
enum ArkReadError {
    IoError(io::Error)
}

fn read_ark_from_file_path(path: &Path) -> Result<Vec<u8>, ArkReadError> {
    fs::read(path).map_err(|e| ArkReadError::IoError(e))
}
