use crate::byte_utility::{split, split_as_u32};
use super::opcode::FuncId;

/// Represents a function section.
#[derive(Debug, PartialEq, Eq)]
pub struct FunctionSection {
    pub functions_length: u32,
    pub functions: Vec<Function>,
}

/// An error from reading an invalid function section.
#[derive(Debug, PartialEq, Eq)]
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
#[derive(Debug, PartialEq, Eq)]
pub struct Function {
    id: FuncId,
    name_index: u32,
    arity: u32,
    locals_count: u32,
    address: u32,
}

/// An error from reading an invalid error.
#[derive(Debug, PartialEq, Eq)]
pub enum FunctionError {
    MissingId,
    MissingNameIndex,
    MissingArity,
    MissingLocalsCount,
    MissingAddress,
}

impl Function {
    /// Attempts to construct a function from a sequence of bytes.
    /// Reads only the amount of bytes specified by the function itself
    /// and returns the rest.
    pub fn from_bytes(bytes: &[u8]) -> Result<(Function, &[u8]), FunctionError> {
        let (id, bytes) = split_as_u32(bytes)
            .ok_or(FunctionError::MissingId)?;

        let (name_index, bytes) = split_as_u32(bytes)
            .ok_or(FunctionError::MissingNameIndex)?;

        let (arity, bytes) = split_as_u32(bytes)
            .ok_or(FunctionError::MissingArity)?;

        let (locals_count, bytes) = split_as_u32(bytes)
            .ok_or(FunctionError::MissingLocalsCount)?;

        let (address, rest) = split_as_u32(bytes)
            .ok_or(FunctionError::MissingAddress)?;

        let function = Self {
            id: FuncId::from(id),
            name_index,
            arity,
            locals_count,
            address
        };
        Ok((function, rest))
    }

    /// Gets the ID of the function.
    pub fn id(&self) -> FuncId {
        self.id
    }

    /// Gets the name index of the function.
    pub fn name_index(&self) -> u32 {
        self.name_index
    }

    /// Gets the arity of the function.
    pub fn arity(&self) -> u32 {
        self.arity
    }

    /// Gets the amount of locals allocated by the function.
    pub fn locals_count(&self) -> u32 {
        self.locals_count
    }

    /// Gets the bytecode address at which the function starts..
    pub fn address(&self) -> u32 {
        self.address
    }
}
