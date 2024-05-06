use crate::byte_utility::{get, split_const, split_as_u32};

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

pub type Address = u32;
pub type FuncId = u32;
pub type VarIndex = u32;

/// A single op-code. Some op-codes have associated operand data.
#[repr(u8)]
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub enum Opcode {
    NoOp = NO_OP,

    // Control flow
    Jump(Address) = JUMP,
    JumpIf(Address) = JUMP_IF,
    Call(FuncId) = CALL,
    Ret = RET,

    // Stack operations
    PushInt(i32) = PUSH_INT,
    PushBool(bool) = PUSH_BOOL,
    PushFunc(u32) = PUSH_FUNC,
    PushNil = PUSH_NIL,
    Pop = POP,
    Dup = DUP,
    Swap = SWAP,

    // Locals operations
    StoreVar(VarIndex) = STORE_VAR,
    LoadVar(VarIndex) = LOAD_VAR,

    // Value operations
    Add = ADD,
    Sub = SUB,
    Mult = MULT,
    Div = DIV,
    Equal = EQUAL,
    LessThan = LESS_THAN,
    Not = NOT,
    And = AND,
    Or = OR,
}

/// An error from reading an invalid opcode.
#[derive(Debug, PartialEq, Eq)]
pub enum OpcodeError {
    Empty,
    OutOfRange,
    TooLittleData,
    BadData,
}

impl Opcode {
    /// Attempts to construct an op-code from a sequence of bytes.
    /// If the function succeeds then [`Ok`] is returned with the constructed
    /// op-code and the remaining unread bytes, otherwise an [`OpcodeError`] is returned.
    /// Reads only the amount of bytes required by the opcode and returns the rest.
    pub fn from_bytes(bytes: &[u8]) -> Result<(Self, &[u8]), OpcodeError> {
        let (code, data) = get(bytes, 0)
            .ok_or(OpcodeError::Empty)?;

        if let Some(simple) = match *code {
            self::NO_OP => Some(Opcode::NoOp),
            self::RET => Some(Opcode::Ret),
            self::PUSH_NIL => Some(Opcode::PushNil),
            self::POP => Some(Opcode::Pop),
            self::DUP => Some(Opcode::Dup),
            self::SWAP => Some(Opcode::Swap),
            self::ADD => Some(Opcode::Add),
            self::SUB => Some(Opcode::Sub),
            self::MULT => Some(Opcode::Mult),
            self::DIV => Some(Opcode::Div),
            self::EQUAL => Some(Opcode::Equal),
            self::LESS_THAN => Some(Opcode::LessThan),
            self::NOT => Some(Opcode::Not),
            self::AND => Some(Opcode::And),
            self::OR => Some(Opcode::Or),
            _ => None
        } {
            return Ok((simple, data));
        }

        match *code {
            self::JUMP => {
                let (address, rest) = split_as_u32(data)
                    .ok_or(OpcodeError::TooLittleData)?;
                Ok((Opcode::Jump(address), rest))
            },
            self::JUMP_IF => {
                let (address, rest) = split_as_u32(data)
                    .ok_or(OpcodeError::TooLittleData)?;
                Ok((Opcode::JumpIf(address), rest))
            },
            self::CALL => {
                let (arg_count, rest) = split_as_u32(data)
                    .ok_or(OpcodeError::TooLittleData)?;
                Ok((Opcode::Call(arg_count), rest))
            },

            self::PUSH_INT => {
                let (bytes, rest) = split_const::<4>(data)
                    .ok_or(OpcodeError::TooLittleData)?;
                let value = i32::from_be_bytes(*bytes);
                Ok((Opcode::PushInt(value), rest))
            },
            self::PUSH_BOOL => {
                let (byte, rest) = get(data, 0)
                    .ok_or(OpcodeError::TooLittleData)?;
                let value = match byte {
                    0 => false,
                    1 => true,
                    _ => return Err(OpcodeError::BadData),
                };
                Ok((Opcode::PushBool(value), rest))
            },
            self::PUSH_FUNC => {
                let (func_id, rest) = split_as_u32(data)
                    .ok_or(OpcodeError::TooLittleData)?;
                Ok((Opcode::PushFunc(func_id), rest))
            },

            self::STORE_VAR => {
                let (var_id, rest) = split_as_u32(data)
                    .ok_or(OpcodeError::TooLittleData)?;
                Ok((Opcode::StoreVar(var_id), rest))
            },
            self::LOAD_VAR => {
                let (var_id, rest) = split_as_u32(data)
                    .ok_or(OpcodeError::TooLittleData)?;
                Ok((Opcode::LoadVar(var_id), rest))
            },

            _ => Err(OpcodeError::OutOfRange)
        }
    }
}

impl TryFrom<&[u8]> for Opcode {
    type Error = OpcodeError;

    fn try_from(value: &[u8]) -> Result<Self, Self::Error> {
        let (x, _) = Self::from_bytes(value)?;
        Ok(x)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    const TEST_CONST_U32: u32 = 0x0E060201;
    const TEST_CONST_I32: i32 = TEST_CONST_U32 as i32;

    fn test_simple(byte: u8, opcode: Opcode) {
        let bytes = &[byte, 255];
        let (x, rest) = Opcode::from_bytes(bytes).unwrap();
        assert_eq!(x, opcode);
        assert_eq!(rest, &[255]);
    }

    fn test_data(byte: u8, mut data: Vec<u8>, opcode: Opcode) {
        let mut bytes = vec![byte];
        bytes.append(&mut data);
        bytes.push(255);

        let bytes = bytes.as_slice();

        let (x, rest) = Opcode::from_bytes(bytes).unwrap();

        assert_eq!(x, opcode);
        assert_eq!(rest, &[255]);
    }

    fn test_u32_data(byte: u8, opcode: Opcode) {
        test_data(byte, vec![0xe, 6, 2, 1], opcode);
    }

    fn test_too_little_data(byte: u8) {
        let bytes = &[byte];
        let x = Opcode::from_bytes(bytes).unwrap_err();
        assert_eq!(x, OpcodeError::TooLittleData);
    }

    #[test]
    fn noop() {
        test_simple(NO_OP, Opcode::NoOp);
    }

    #[test]
    fn jump() {
        test_u32_data(JUMP, Opcode::Jump(TEST_CONST_U32));
    }

    #[test]
    fn jump_too_little_data() {
        test_too_little_data(JUMP);
    }

    #[test]
    fn jump_if() {
        test_u32_data(JUMP_IF, Opcode::JumpIf(TEST_CONST_U32));
    }

    #[test]
    fn jump_if_too_little_data() {
        test_too_little_data(JUMP_IF);
    }

    #[test]
    fn call() {
        test_u32_data(CALL, Opcode::Call(TEST_CONST_U32));
    }

    #[test]
    fn call_too_little_data() {
        test_too_little_data(CALL);
    }

    #[test]
    fn ret() {
        test_simple(RET, Opcode::Ret);
    }

    #[test]
    fn push_int() {
        test_u32_data(PUSH_INT, Opcode::PushInt(TEST_CONST_I32));
    }

    #[test]
    fn push_int_too_little_data() {
        test_too_little_data(PUSH_INT);
    }

    #[test]
    fn push_bool_true() {
        test_data(PUSH_BOOL, vec![1], Opcode::PushBool(true));
    }

    #[test]
    fn push_bool_false() {
        test_data(PUSH_BOOL, vec![0], Opcode::PushBool(false));
    }

    #[test]
    fn push_bool_invalid() {
        let bytes: &[u8] = &[PUSH_BOOL, 2];
        let x = Opcode::from_bytes(bytes).unwrap_err();
        assert_eq!(x, OpcodeError::BadData);
    }

    #[test]
    fn push_bool_too_little_data() {
        test_too_little_data(PUSH_BOOL);
    }

    #[test]
    fn push_func() {
        test_u32_data(PUSH_FUNC, Opcode::PushFunc(TEST_CONST_U32));
    }

    #[test]
    fn push_func_too_little_data() {
        test_too_little_data(PUSH_FUNC);
    }

    #[test]
    fn push_nil() {
        test_simple(PUSH_NIL, Opcode::PushNil);
    }

    #[test]
    fn push_pop() {
        test_simple(POP, Opcode::Pop);
    }

    #[test]
    fn push_dup() {
        test_simple(DUP, Opcode::Dup);
    }

    #[test]
    fn push_swap() {
        test_simple(SWAP, Opcode::Swap);
    }

    #[test]
    fn push_store_var() {
        test_u32_data(STORE_VAR, Opcode::StoreVar(TEST_CONST_U32));
    }

    #[test]
    fn push_store_var_too_little_data() {
        test_too_little_data(STORE_VAR);
    }

    #[test]
    fn push_load_var() {
        test_u32_data(LOAD_VAR, Opcode::LoadVar(TEST_CONST_U32));
    }

    #[test]
    fn push_load_var_too_little_data() {
        test_too_little_data(LOAD_VAR);
    }

    #[test]
    fn push_add() {
        test_simple(ADD, Opcode::Add);
    }

    #[test]
    fn push_sub() {
        test_simple(SUB, Opcode::Sub);
    }

    #[test]
    fn push_mult() {
        test_simple(MULT, Opcode::Mult);
    }

    #[test]
    fn push_div() {
        test_simple(DIV, Opcode::Div);
    }

    #[test]
    fn push_less_than() {
        test_simple(LESS_THAN, Opcode::LessThan);
    }

    #[test]
    fn push_equal() {
        test_simple(EQUAL, Opcode::Equal);
    }

    #[test]
    fn push_not() {
        test_simple(NOT, Opcode::Not);
    }

    #[test]
    fn push_and() {
        test_simple(AND, Opcode::And);
    }

    #[test]
    fn push_or() {
        test_simple(OR, Opcode::Or);
    }
}
