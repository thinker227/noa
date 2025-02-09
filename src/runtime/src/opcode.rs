pub const NO_OP: u8 = 0x0;
pub const JUMP: u8 = 0x1;
pub const JUMP_IF: u8 = 0x2;
pub const CALL: u8 = 0x3;
pub const RET: u8 = 0x4;
pub const ENTER_TEMP_FRAME: u8 = 0x5;
pub const EXIT_TEMP_FRAME: u8 = 0x6;
pub const PUSH_INT: u8 = 0x14;
pub const PUSH_BOOL: u8 = 0x15;
pub const PUSH_FUNC: u8 = 0x16;
pub const PUSH_NIL: u8 = 0x17;
pub const PUSH_STRING: u8 = 0x18;
pub const POP: u8 = 0x32;
pub const DUP: u8 = 0x33;
pub const SWAP: u8 = 0x34;
pub const STORE_VAR: u8 = 0x46;
pub const LOAD_VAR: u8 = 0x47;
pub const ADD: u8 = 0x64;
pub const SUB: u8 = 0x65;
pub const MULT: u8 = 0x66;
pub const DIV: u8 = 0x67;
pub const EQUAL: u8 = 0x68;
pub const LESS_THAN: u8 = 0x69;
pub const NOT: u8 = 0x6A;
pub const AND: u8 = 0x6B;
pub const OR: u8 = 0x6C;
pub const GREATER_THAN: u8 = 0x6D;
pub const CONCAT: u8 = 0x6E;
pub const TO_STRING: u8 = 0x6F;
pub const BOUNDARY: u8 = 0xFF;
