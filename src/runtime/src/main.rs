#![allow(dead_code, unused_variables)]

use std::{fs, io, path::Path};

use ark::Ark;
use clap::Parser;
use cli::Args;

use crate::runtime::virtual_machine::VM;

mod cli;
mod byte_utility;
mod ark;
mod runtime;

fn main() {
    let args = Args::parse();

    let ark_bytes = if let Some(path) = &args.bytecode_file_path {
        read_ark_from_file_path(path)
    } else {
        println!(".ark file path was not provided");
        return;
    };

    match ark_bytes {
        Ok(ark_bytes) => {
            let ark = match Ark::from_bytes(ark_bytes.as_slice()) {
                Ok(ark) => ark,
                Err(_) => {
                    println!("Error reading .ark file.");
                    return;
                }
            };

            execute(ark);
        }
        Err(e) => match e {
            ArkReadError::IoError(e) => println!("{e}"),
        }
    }
}

fn execute(ark: Ark) -> () {
    let mut vm = VM::new(ark, 2_000, 10_000);
    vm.execute_main();
}

#[derive(Debug)]
enum ArkReadError {
    IoError(io::Error)
}

fn read_ark_from_file_path(path: &Path) -> Result<Vec<u8>, ArkReadError> {
    fs::read(path).map_err(|e| ArkReadError::IoError(e))
}
