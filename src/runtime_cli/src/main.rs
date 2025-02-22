#![feature(try_trait_v2)]

use std::{fs, io::Cursor};

use args::Args;
use exit::{Exit, IntoExit};
use binrw::BinRead;
use clap::Parser;

use noa_runtime::exception::FormattedException;
use noa_runtime::value::Value;
use noa_runtime::vm::Vm;
use noa_runtime::ark::{Ark, CodeSection, FuncId, FunctionSection, Header, StringSection};

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
        header: Header {
            main,
            ..
        },
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

    let mut vm = Vm::new(
        functions,
        strings,
        code,
        100_000,
        10_000,
        100_000
    );

    let result = run(&mut vm, main, args.print_return_value);

    match result {
        Ok(_) => {},
        Err(ex) => print_exception(ex),
    }

    Exit::ok()
}

fn run(vm: &mut Vm, main: FuncId, print_ret: bool) -> Result<(), FormattedException> {
    let result = vm.call_run(main.into(), &[])?;

    if print_ret {
        let str = vm.to_string(result)?;
        println!("{str}");
    }

    Ok(())
}

fn print_exception(ex: FormattedException) {
    println!("An exception occurred:");
    println!();
    println!("  {}", ex.exception);
    println!();

    for frame in ex.stack_trace {
        if let Some(address) = frame.address {
            println!("    in {} at 0x{:X}", frame.function, address);
        } else {
            println!("    in {}", frame.function);
        }
    }

    println!();
}
