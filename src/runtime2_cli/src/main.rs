#![feature(try_trait_v2)]

use std::{fs, io::Cursor};

use args::Args;
use exit::{Exit, IntoExit};
use binrw::BinRead;
use clap::Parser;

use noa_runtime::exception::FormattedException;
use noa_runtime::value::Value;
use noa_runtime::vm::{self, Vm};
use noa_runtime::ark::{Ark, CodeSection, FunctionSection, StringSection};

mod args;
mod exit;

fn main() -> Exit<()> {
    let args = Args::try_parse().into_exit()?;

    let ark = {
        let bytes = fs::read(args.ark_file_path).into_exit()?;
        let mut cursor = Cursor::new(bytes);
        Ark::read_be(&mut cursor).into_exit()?
    };

    let Ark {
        function_section: FunctionSection {
            functions,
            ..
        },
        code_section: CodeSection {
            code,
            ..
        },
        string_section: StringSection {
            strings,
            ..
        },
        ..
    } = ark;

    let res = vm::execute(
        functions,
        strings,
        code,
        100_000,
        10_000,
        100_000,
        run
    );

    Exit::ok()
}

fn run(vm: &mut Vm) -> Result<Value, FormattedException> {
    todo!()
}
