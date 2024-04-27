use std::{fs, io, path::Path};

use clap::Parser;
use cli::Args;

mod cli;
mod runtime;

fn main() {
    let args = Args::parse();

    let bytecode = if let Some(path) = &args.bytecode_file_path {
        read_bytecode_from_file_path(path)
    } else {
        todo!("Handle file path not being provided.")
    };

    println!("{:?}", bytecode);
}

#[derive(Debug)]
enum BytecodeReadError {
    IoError(io::Error)
}

fn read_bytecode_from_file_path(path: &Path) -> Result<Vec<u8>, BytecodeReadError> {
    fs::read(path).map_err(|e| BytecodeReadError::IoError(e))
}
