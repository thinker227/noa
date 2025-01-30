#![feature(try_trait_v2)]

use std::{fs, io::Cursor};

use args::Args;
use exit::{Exit, IntoExit};
use binrw::BinRead;
use clap::Parser;
use noa_runtime::ark::Ark;

mod args;
mod exit;

fn main() -> Exit<()> {
    let args = Args::try_parse().into_exit()?;

    let _ark = {
        let bytes = fs::read(args.ark_file_path).into_exit()?;
        let mut cursor = Cursor::new(bytes);
        Ark::read_be(&mut cursor).into_exit()?
    };

    Exit::ok()
}
