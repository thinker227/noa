use std::path::PathBuf;

use clap::Parser;

#[derive(Parser, Debug)]
#[command(version = "1", about = "Noa runtime")]
pub struct Args {
    /// The bytecode file to execute
    #[arg(short = 'f', value_name = ".ark file", value_parser = file_exists)]
    pub bytecode_file_path: Option<PathBuf>
}

fn file_exists(s: &str) -> Result<PathBuf, String> {
    let path = PathBuf::from(s);

    if !path.exists() {
        return Err(String::from("Path doesn't exist"));
    }

    if !path.is_file() {
        return Err(String::from("Path is not a file"))
    }

    Ok(path)
}