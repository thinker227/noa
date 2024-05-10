use super::{opcode::Opcode, virtual_machine::VM};

pub fn print_disassembly(vm: &VM) {
    todo!();
}

pub fn disassemble_opcode(opcode: Opcode, vm: &VM) -> String {
    match opcode {
        Opcode::NoOp => "noop".into(),
        Opcode::Jump(address) => format!("jump <to_address: {address}>"),
        Opcode::JumpIf(address) => format!("jump_if <to_address: {address}>"),
        Opcode::Call(arg_count) => format!("call <argument_count: {arg_count}>"),
        Opcode::Ret => "return".into(),
        Opcode::PushInt(x) => format!("push_int <value: {x}>"),
        Opcode::PushBool(x) => format!("push_bool <value: {x}>"),
        Opcode::PushFunc(id) => {
            let function_name = match vm.functions().get(&id) {
                Some(f) => vm.get_string_or_fallback(f.name_index(), "?"),
                None => "!!!INVALID!!!"
            };
            format!("push_function <function_id: {id} ({function_name})>")
        },
        Opcode::PushNil => "push_nil".into(),
        Opcode::Pop => "pop".into(),
        Opcode::Dup => "duplicate".into(),
        Opcode::Swap => "swap".into(),
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
        Opcode::GreaterThan => "greater_than".into(),
    }
}
