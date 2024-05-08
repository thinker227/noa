#![allow(dead_code, unused_variables)]

use std::{fs, io, path::Path};

use clap::Parser;
use cli::Args;

use ark::Ark;
use runtime::virtual_machine::VM;

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

            execute(ark, args.print_return_value);
        }
        Err(e) => match e {
            ArkReadError::IoError(e) => {
                eprintln!("{e}");
                std::process::exit(1);
            }
        }
    }
}

fn execute(ark: Ark, print_return_value: bool) -> () {
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

            std::process::exit(exit_code);
        },
        Err(e) => {
            eprintln!("An exception occurred!");
            eprintln!("  {}", e.kind().to_string());
            eprintln!();
            eprintln!("  Stack trace:");
            for f in e.stack_trace() {
                let function = vm.functions().get(&f.function).unwrap();
                let func_name = vm.get_string(function.name_index()).unwrap();
                eprintln!("    at address 0x{0:x} in {1}", f.address, func_name);
            }

            std::process::exit(1);
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
