#![feature(try_trait_v2)]

use std::fs;
use std::io::{self, Cursor};

use args::Args;
use exit::{Exit, IntoExit};
use binrw::BinRead;
use clap::Parser;

use noa_debugger_tui::DebuggerTui;
use noa_runtime::exception::FormattedException;
use noa_runtime::vm::debugger::Debugger;
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

    let debugger: Option<Box<dyn Debugger>> = if args.debug {
        Some(Box::new(
            DebuggerTui::new(io::stdout())
                .into_exit()?
        ))
    } else {
        None
    };

    let mut vm = Vm::new(
        functions,
        strings,
        code,
        100_000,
        10_000,
        100_000,
        debugger
    );

    let result = run(&mut vm, main, args.print_return_value);

    match result {
        Ok(_) => {},
        Err(ex) => print_exception(ex),
    }

    Exit::ok()
}

fn run(vm: &mut Vm, main: FuncId, print_ret: bool) -> Result<(), FormattedException> {
    if let Some(debugger) = vm.debugger() {
        debugger.init();
    }

    let result = vm.call_run(main.into(), &[])?;

    if let Some(debugger) = vm.debugger() {
        debugger.exit();
    }

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
