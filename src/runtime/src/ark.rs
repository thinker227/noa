use std::string::FromUtf8Error;

use binrw::binread;

#[derive(Debug)]
#[binread]
pub struct Ark {
    pub header: Header,
    pub function_section: FunctionSection,
    pub code_section: CodeSection,
    pub string_section: StringSection,
}

#[derive(Debug)]
#[binread]
pub struct Header {
    pub identifier: Identifier,
    pub main: FuncId,
}

#[derive(Debug)]
#[binread]
#[br(magic = b"totheark")]
pub struct Identifier;

/// An encoded ID of a function.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
#[binread]
pub struct FuncId(pub u32);

impl FuncId {
    const MSB: u32 = u32::MAX << (u32::BITS - 1);

    /// Returns whether the function is native or not.
    pub fn is_native(&self) -> bool {
        self.0 & Self::MSB == Self::MSB
    }

    /// Decodes the ID into a non-encoded format.
    pub fn decode(&self) -> u32 {
        self.0 & !Self::MSB
    }
}

#[derive(Debug)]
#[binread]
pub struct Function {
    pub id: FuncId,
    pub name_index: u32,
    pub arity: u32,
    pub locals_count: u32,
    pub address: u32,
}

#[derive(Debug)]
#[binread]
pub struct FunctionSection {
    pub length: u32,
    #[br(count = length)]
    pub functions: Vec<Function>,
}

#[derive(Debug)]
#[binread]
pub struct CodeSection {
    pub length: u32,
    #[br(count = length)]
    pub code: Vec<u8>,
}

#[derive(Debug)]
#[binread]
pub struct StringSection {
    pub length: u32,
    #[br(count = length)]
    #[br(try_map = map_strings)]
    pub strings: Vec<String>,
}

fn map_strings(ss: Vec<LenString>) -> Result<Vec<String>, FromUtf8Error> {
    ss.into_iter()
        .map(|s| String::from_utf8(s.bytes))
        .collect()
}

#[allow(dead_code)]
#[derive(Debug)]
#[binread]
struct LenString {
    pub length: u32,
    #[br(count = length)]
    pub bytes: Vec<u8>,
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn funcid_is_native() {
        let id = FuncId(0b00000000_00000000_00000010_01101101);
        assert!(!id.is_native());

        let id = FuncId(0b10000000_00000000_00000011_10011110);
        assert!(id.is_native());
    }

    #[test]
    fn funcid_decode() {
        let id = FuncId(0b00000000_00000000_00000010_01101101);
        assert_eq!(id.decode(), 621);

        let id = FuncId(0b10000000_00000000_00000011_10011110);
        assert_eq!(id.decode(), 926);
    }
}
