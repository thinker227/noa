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
    #[br(magic = b"totheark")]
    pub identifier: [u8; 8],
    pub main: FuncId,
}

#[derive(Debug)]
#[binread]
pub struct FuncId(pub u32);

#[derive(Debug)]
#[binread]
pub struct Function {
    pub id: u32,
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
