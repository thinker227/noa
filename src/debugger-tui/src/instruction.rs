use noa_runtime::vm::debugger::DebugInspection;
use noa_runtime::value::{Type, Value};
use noa_runtime::opcode;

pub struct InstructionSummary {
    pub _opcode: u8,
    pub name: String,
    pub operands: Vec<Operand>,
    pub arguments: Vec<Argument>,
}

pub struct Operand {
    pub name: String,
    pub length: usize,
    pub typ: String,
    pub value: Option<String>,
}

pub struct Argument {
    pub name: String,
    pub expected_type: Option<Type>,
    pub value: Option<Value>,
}

impl<'insp, 'vm> From<&'insp DebugInspection<'vm>> for InstructionSummary {
    fn from(inspection: &DebugInspection) -> Self {
        let opcode = inspection.consts.code[inspection.ip];

        let (
            name,
            operands,
            arguments
        ) = match opcode {
            opcode::NO_OP => (
                "NoOp",
                vec![],
                vec![]
            ),
            opcode::JUMP => (
                "Jump",
                vec![
                    make_operand::<u32>(inspection, "address")
                ],
                vec![]
            ),
            opcode::JUMP_IF => (
                "JumpIf",
                vec![
                    make_operand::<u32>(inspection, "address")
                ],
                vec![
                    make_arg(
                        inspection,
                        0,
                        "condition",
                        Some(Type::Bool)
                    )
                ]
            ),
            opcode::CALL => {
                let value = read_operand::<u32>(inspection);

                let args = if let Some(arg_count) = value {
                    let mut args = Vec::new();
                    let mut i = 0;
                    args.resize_with(arg_count as usize, || {
                        let arg = make_arg(
                            inspection,
                            arg_count as usize - i - 1,
                            format!("arg {i}"),
                            None
                        );
                        i += 1;
                        arg
                    });
                    args
                } else {
                    vec![]
                };

                let operand = operand_from::<u32>(value, "arg count");

                (
                    "Call",
                    vec![operand],
                    args
                )
            },
            opcode::RET => (
                "Ret",
                vec![],
                vec![
                    make_arg(
                        inspection,
                        0,
                        "val",
                        None
                    )
                ]
            ),
            opcode::ENTER_TEMP_FRAME => (
                "EnterTempFrame",
                vec![],
                vec![]
            ),
            opcode::EXIT_TEMP_FRAME => (
                "ExitTempFrame",
                vec![],
                vec![]
            ),
            opcode::PUSH_FLOAT => (
                "PushFloat",
                vec![
                    make_operand::<f64>(inspection, "val")
                ],
                vec![]
            ),
            opcode::PUSH_BOOL => (
                "PushBool",
                vec![
                    make_operand::<bool>(inspection, "val")
                ],
                vec![]
            ),
            opcode::PUSH_FUNC => (
                "PushFunc",
                vec![
                    make_operand::<u32>(inspection, "func id")
                ],
                vec![]
            ),
            opcode::PUSH_NIL => (
                "PushNil",
                vec![],
                vec![]
            ),
            opcode::PUSH_STRING => (
                "PushString",
                vec![
                    make_operand::<u32>(inspection, "string index")
                ],
                vec![]
            ),
            opcode::PUSH_OBJECT => (
                "PushObject",
                vec![
                    make_operand::<bool>(inspection, "dynamic")
                ],
                vec![]
            ),
            opcode::PUSH_LIST => (
                "PushList",
                vec![],
                vec![]
            ),
            opcode::POP => (
                "Pop",
                vec![],
                vec![]
            ),
            opcode::DUP => (
                "Dup",
                vec![],
                vec![]
            ),
            opcode::SWAP => (
                "Swap",
                vec![],
                vec![]
            ),
            opcode::STORE_VAR => (
                "StoreVar",
                vec![
                    make_operand::<u32>(inspection, "var index")
                ],
                vec![]
            ),
            opcode::LOAD_VAR => (
                "LoadVar",
                vec![
                    make_operand::<u32>(inspection, "var index")
                ],
                vec![]
            ),
            opcode::ADD => (
                "Add",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Number)),
                    make_arg(inspection, 0, "right", Some(Type::Number))
                ]
            ),
            opcode::SUB => (
                "Sub",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Number)),
                    make_arg(inspection, 0, "right", Some(Type::Number))
                ]
            ),
            opcode::MULT => (
                "Mult",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Number)),
                    make_arg(inspection, 0, "right", Some(Type::Number))
                ]
            ),
            opcode::DIV => (
                "Div",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Number)),
                    make_arg(inspection, 0, "right", Some(Type::Number))
                ]
            ),
            opcode::EQUAL => (
                "Equal",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Bool)),
                    make_arg(inspection, 0, "right", Some(Type::Bool))
                ]
            ),
            opcode::LESS_THAN => (
                "LessThan",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Number)),
                    make_arg(inspection, 0, "right", Some(Type::Number))
                ]
            ),
            opcode::NOT => (
                "Not",
                vec![],
                vec![
                    make_arg(inspection, 0, "val", Some(Type::Bool))
                ]
            ),
            opcode::AND => (
                "And",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Bool)),
                    make_arg(inspection, 0, "right", Some(Type::Bool))
                ]
            ),
            opcode::OR => (
                "Or",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Bool)),
                    make_arg(inspection, 0, "right", Some(Type::Bool))
                ]
            ),
            opcode::GREATER_THAN => (
                "GreaterThan",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::Number)),
                    make_arg(inspection, 0, "right", Some(Type::Number))
                ]
            ),
            opcode::CONCAT => (
                "Concat",
                vec![],
                vec![
                    make_arg(inspection, 1, "left", Some(Type::String)),
                    make_arg(inspection, 0, "right", Some(Type::String))
                ]
            ),
            opcode::TO_STRING => (
                "ToString",
                vec![],
                vec![
                    make_arg(inspection, 0, "val", None)
                ]
            ),
            opcode::ADD_FIELD => (
                "AddField",
                vec![
                    make_operand::<bool>(inspection, "mutable")
                ],
                vec![
                    make_arg(inspection, 2, "object", Some(Type::Object)),
                    make_arg(inspection, 1, "field", Some(Type::String)),
                    make_arg(inspection, 0, "value", None)
                ]
            ),
            opcode::WRITE_FIELD => (
                "WriteField",
                vec![],
                vec![
                    make_arg(inspection, 2, "object", Some(Type::Object)),
                    make_arg(inspection, 1, "field", Some(Type::String)),
                    make_arg(inspection, 0, "value", None)
                ]
            ),
            opcode::READ_FIELD => (
                "ReadField",
                vec![],
                vec![
                    make_arg(inspection, 1, "object", Some(Type::Object)),
                    make_arg(inspection, 0, "field", Some(Type::String))
                ]
            ),
            opcode::APPEND_ELEMENT => (
                "AppendElement",
                vec![],
                vec![
                    make_arg(inspection, 1, "list", Some(Type::List)),
                    make_arg(inspection, 0, "value", None)
                ]
            ),
            opcode::WRITE_ELEMENT => (
                "WriteElement",
                vec![],
                vec![
                    make_arg(inspection, 2, "list", Some(Type::List)),
                    make_arg(inspection, 1, "index", Some(Type::Number)),
                    make_arg(inspection, 0, "value", None)
                ]
            ),
            opcode::READ_ELEMENT => (
                "ReadElement",
                vec![],
                vec![
                    make_arg(inspection, 1, "list", Some(Type::List)),
                    make_arg(inspection, 0, "index", Some(Type::Number))
                ]
            ),
            opcode::BOUNDARY => (
                "Boundary",
                vec![],
                vec![]
            ),
            _ => (
                "InvalidOpcode",
                vec![],
                vec![]
            )
        };

        InstructionSummary {
            name: name.into(),
            _opcode: opcode,
            operands,
            arguments
        }
    }
}

fn make_operand<'insp, 'vm, T: IntoOperand>(
    inspection: &'insp DebugInspection<'vm>,
    name: impl ToString
) -> Operand
{
    Operand {
        name: name.to_string(),
        length: T::length(),
        typ: T::typ(),
        value: read_operand::<T>(inspection)
            .map(|x| x.show())
    }
}

fn operand_from<T: IntoOperand>(
    value: Option<impl ToString>,
    name: impl ToString
) -> Operand
{
    Operand {
        name: name.to_string(),
        length: T::length(),
        typ: T::typ(),
        value: value.map(|x| x.to_string())
    }
}

fn read_operand<'insp, 'vm, T: IntoOperand>(
    inspection: &'insp DebugInspection<'vm>
) -> Option<T>
{
    let ip = inspection.ip;
    let remaining_bytes = &inspection.consts.code[(ip + 1)..];
    let length = T::length();
    
    if remaining_bytes.len() >= length {
        let bytes = &remaining_bytes[..length];
        let value = T::read(bytes);
        Some(value)
    } else {
        None
    }
}

fn make_arg<'insp, 'vm>(
    inspection: &'insp DebugInspection<'vm>,
    index: usize,
    name: impl ToString,
    expected_type: Option<Type>
) -> Argument {
    let value = inspection.stack.get(inspection.stack.head() - index - 1).copied();

    // let value = if let Some((start, _)) = var_indices {
    //     inspection.stack.get(start + index).copied()
    // } else {
    //     None
    // };

    Argument {
        name: name.to_string(),
        expected_type,
        value
    }
}

trait IntoOperand: Sized {
    fn typ() -> String;

    fn length() -> usize;

    fn read(bytes: &[u8]) -> Self;

    fn show(&self) -> String;
}

impl IntoOperand for u32 {
    fn typ() -> String {
        "u32".into()
    }

    fn length() -> usize {
        4
    }

    fn read(bytes: &[u8]) -> Self {
        Self::from_be_bytes(bytes.try_into().unwrap())
    }

    fn show(&self) -> String {
        format!("{} 0x{:X}", self, self)
    }
}

impl IntoOperand for f64 {
    fn typ() -> String {
        "f64".into()
    }

    fn length() -> usize {
        8
    }

    fn read(bytes: &[u8]) -> Self {
        Self::from_be_bytes(bytes.try_into().unwrap())
    }

    fn show(&self) -> String {
        self.to_string()
    }
}

impl IntoOperand for bool {
    fn typ() -> String {
        "bool".into()
    }

    fn length() -> usize {
        1
    }

    fn read(bytes: &[u8]) -> Self {
        bytes[0] != 0
    }

    fn show(&self) -> String {
        self.to_string()
    }
}
