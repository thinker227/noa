use crate::byte_utility::{split, split_as_u32};
use super::opcode::{FuncId, Opcode, OpcodeError};

/// Represents a function section.
#[derive(Debug)]
pub struct FunctionSection {
    pub functions_length: u32,
    pub functions: Vec<Function>,
}

/// An error from reading an invalid function section.
#[derive(Debug)]
pub enum FunctionSectionError {
    MissingLength,
    IncongruentLength,
    FunctionError(FunctionError),
}

impl FunctionSection {
    /// Attempts to construct a function section from a sequence of bytes.
    /// Reads only the amount of bytes specified by the section itself
    /// and returns the rest.
    pub fn from_bytes(bytes: &[u8]) -> Result<(FunctionSection, &[u8]), FunctionSectionError> {
        let (functions_length, rest) = split_as_u32(bytes)
            .ok_or(FunctionSectionError::MissingLength)?;
        
        let (functions_bytes, rest) = split(rest, functions_length as usize)
            .ok_or(FunctionSectionError::IncongruentLength)?;

        let functions = parse_functions(functions_bytes)
            .map_err(|e| FunctionSectionError::FunctionError(e))?;

        let section = Self {
            functions_length,
            functions
        };
        Ok((section, rest))
    }
}

fn parse_functions(mut bytes: &[u8]) -> Result<Vec<Function>, FunctionError> {
    let mut functions = Vec::new();

    while !bytes.is_empty() {
        let (function, rest) = Function::from_bytes(bytes)?;

        functions.push(function);

        bytes = rest;
    }

    Ok(functions)
}

/// A function executed by the runtime.
#[derive(Debug)]
pub struct Function {
    id: FuncId,
    code_length: u32,
    code: Vec<Opcode>,
}

/// An error from reading an invalid error.
#[derive(Debug)]
pub enum FunctionError {
    MissingId,
    MissingCodeLength,
    IncongruentLength,
    OpcodeError(OpcodeError),
}

impl Function {
    /// Attempts to construct a function from a sequence of bytes.
    /// Reads only the amount of bytes specified by the function itself
    /// and returns the rest.
    pub fn from_bytes(bytes: &[u8]) -> Result<(Function, &[u8]), FunctionError> {
        let (id, bytes) = split_as_u32(bytes)
            .ok_or(FunctionError::MissingId)?;

        let (code_length, bytes) = split_as_u32(bytes)
            .ok_or(FunctionError::MissingCodeLength)?;

        let (code_bytes, rest) = split(bytes, code_length as usize)
            .ok_or(FunctionError::IncongruentLength)?;

        let code = parse_opcodes(code_bytes)
            .map_err(|e| FunctionError::OpcodeError(e))?;

        let function = Self {
            id,
            code_length,
            code
        };
        Ok((function, rest))
    }

    /// Gets the ID of the function.
    pub fn id(&self) -> FuncId {
        self.id
    }

    /// Gets the bytecode of the function.
    pub fn code(&self) -> &Vec<Opcode> {
        &self.code
    }
}

/// Parses opcodes from a sequence of bytes.
/// Parses the *entire* sequence.
fn parse_opcodes(mut bytecode: &[u8]) -> Result<Vec<Opcode>, OpcodeError> {
    let mut opcodes = Vec::new();

    while !bytecode.is_empty() {
        let (opcode, rest) = Opcode::from_bytes(bytecode)?;
        opcodes.push(opcode);
        bytecode = rest;
    }

    Ok(opcodes)
}
