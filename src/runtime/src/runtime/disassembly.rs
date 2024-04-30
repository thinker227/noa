use super::{opcode::Opcode, virtual_machine::VM};

pub fn print_disassembly(vm: &VM) {
    let functions = vm.functions();
    
    println!("Disassembly:");
    for (id, f) in functions {
        let name = vm.get_string(f.name_index()).unwrap();

        println!("  Function <{name}>");
        for (index, code) in f.code().iter().enumerate() {
            println!("    {0}: {1}", index, disassemble_opcode(*code));
        }
    }

    println!();
}

pub fn disassemble_opcode(opcode: Opcode) -> String {
    match opcode {
        Opcode::NoOp => "noop".into(),
        Opcode::Jump(address) => format!("jump <to_address: {address}>"),
        Opcode::JumpIf(address) => format!("jump_if <to_address: {address}>"),
        Opcode::Call(arg_count) => format!("call <argument_count: {arg_count}>"),
        Opcode::Ret => "return".into(),
        Opcode::PushInt(x) => format!("push_int <value: {x}>"),
        Opcode::PushBool(x) => format!("push_bool <value: {x}>"),
        Opcode::PushFunc(id) => format!("push_function <function_id: {id}>"),
        Opcode::PushNil => "push_nil".into(),
        Opcode::Pop => "pop".into(),
        Opcode::Dup => "duplicate".into(),
        Opcode::StoreVar(index) => format!("store_variable <variable_index: {index}>"),
        Opcode::LoadVar(index) => format!("load_variable <variable_index: {index}>"),
        Opcode::Add => "add".into(),
        Opcode::Sub => "subtract".into(),
        Opcode::Mult => "multiply".into(),
        Opcode::Div => "divide".into(),
        Opcode::Equal => "equals".into(),
        Opcode::LessThan => "less_than".into(),
        Opcode::Not => "not".into(),
        Opcode::And => "and".into(),
        Opcode::Or => "or".into(),
    }
}
