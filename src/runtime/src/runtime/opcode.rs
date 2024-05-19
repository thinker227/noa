use crate::wrapper;

pub const NO_OP: u8 = 0;
pub const JUMP: u8 = 1;
pub const JUMP_IF: u8 = 2;
pub const CALL: u8 = 3;
pub const RET: u8 = 4;
pub const PUSH_INT: u8 = 20;
pub const PUSH_BOOL: u8 = 21;
pub const PUSH_FUNC: u8 = 22;
pub const PUSH_NIL: u8 = 23;
pub const POP: u8 = 50;
pub const DUP: u8 = 51;
pub const SWAP: u8 = 52;
pub const STORE_VAR: u8 = 70;
pub const LOAD_VAR: u8 = 71;
pub const ADD: u8 = 100;
pub const SUB: u8 = 101;
pub const MULT: u8 = 102;
pub const DIV: u8 = 103;
pub const EQUAL: u8 = 104;
pub const LESS_THAN: u8 = 105;
pub const NOT: u8 = 106;
pub const AND: u8 = 107;
pub const OR: u8 = 108;
pub const GREATER_THAN: u8 = 109;

wrapper!{
    Address,
    value: usize,
    "@{value}",
    "The address of an opcode."
}

wrapper!{
    FuncId,
    id: u32,
    "<function {id}>",
    "The ID of a function."
}

wrapper!{
    VarIndex,
    index: u32,
    "<var {index}>",
    "The index of a variable on the stack."
}

/// Generates a wrapper struct around a type.
/// 
/// ### Syntax:
/// `name, field: type, display, doc`
/// 
/// ### Arguments:
/// - `name`: The identifier of the struct.
/// - `field`: The identifier for the internal field.
/// - `type`: The wrapped type.
/// - `display`: The format for displaying the struct. Has the field name available as a variable.
/// - `doc`: The documentation for the struct.
#[macro_export]
macro_rules! wrapper {
    ($name:ident, $field:ident: $type:ty, $display:expr, $doc:expr) => {
        #[doc = $doc]
        #[derive(Debug, PartialEq, Eq, Clone, Copy, Hash)]
        pub struct $name {
            $field: $type
        }

        impl $name {
            pub fn $field(&self) -> $type {
                self.$field
            }
        }

        impl std::fmt::Display for $name {
            fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
                let $field = self.$field;
                write!(f, $display)
            }
        }

        impl From<$type> for $name {
            fn from(value: $type) -> Self {
                Self {
                    $field: value
                }
            }
        }
    };
}
