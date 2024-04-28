use std::{fs, io, path::Path};

use clap::Parser;
use cli::Args;

use crate::runtime::virtual_machine::VM;

mod cli;
mod runtime;

fn main() {
    let args = Args::parse();

    let bytecode = if let Some(path) = &args.bytecode_file_path {
        read_bytecode_from_file_path(path)
    } else {
        println!("Bytecode file path was not provided");
        return;
    };

    match bytecode {
        Ok(bytecode) => {
            execute(bytecode.as_slice());
        }
        Err(e) => match e {
            BytecodeReadError::IoError(e) => println!("{e}"),
        }
    }
}

fn execute(bytecode: &[u8]) -> () {
    let _vm = VM::new(bytecode);
}

#[derive(Debug)]
enum BytecodeReadError {
    IoError(io::Error)
}

fn read_bytecode_from_file_path(path: &Path) -> Result<Vec<u8>, BytecodeReadError> {
    fs::read(path).map_err(|e| BytecodeReadError::IoError(e))
}
