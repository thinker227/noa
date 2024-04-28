use crate::runtime::opcode::FuncId;
use crate::runtime::function::{FunctionSection, FunctionSectionError};
use crate::byte_utility::{split_as_u32, split_const};

/// An Ark file.
#[derive(Debug, PartialEq, Eq)]
pub struct Ark {
    header: Header,
    function_section: FunctionSection,
}

/// An error from reading an invalid Ark file.
#[derive(Debug, PartialEq, Eq)]
pub enum ArkError {
    MissingHeader,
    HeaderError(HeaderError),
    FunctionSectionError(FunctionSectionError),
    TooManyBytes,
}

impl Ark {
    /// Attempts to construct an Ark file from a sequence of bytes.
    /// Reads exactly the amount of bytes specified by the file and returns an error
    /// if there are any remaining bytes afterwards.
    pub fn from_bytes(bytes: &[u8]) -> Result<Ark, ArkError> {
        let (header_bytes, bytes) = split_const::<HEADER_SIZE>(bytes)
            .ok_or(ArkError::MissingHeader)?;

        let header = Header::from_bytes(bytes)
            .map_err(|e| ArkError::HeaderError(e))?;

        let (function_section, rest) = FunctionSection::from_bytes(bytes)
            .map_err(|e| ArkError::FunctionSectionError(e))?;

        if *rest != [] {
            return Err(ArkError::TooManyBytes);
        }

        let ark = Self {
            header,
            function_section
        };
        Ok(ark)
    }
}

pub const IDENTIFIER: [u8; 8] = *b"totheark";
pub const HEADER_SIZE: usize = 12;

/// The header of an Ark file.
#[derive(Debug, PartialEq, Eq)]
pub struct Header {
    identifier: [u8; 8],
    main: FuncId,
}

/// An error from reading an invalid header.
#[derive(Debug, PartialEq, Eq)]
pub enum HeaderError {
    MissingIdentifier,
    BadIdentifier,
    MissingMain,
    TooManyBytes,
}

impl Header {
    /// Attempts to construct a header from a sequence of bytes.
    /// Reads exactly [`HEADER_SIZE`] amount of bytes and returns an error
    /// if there are any remaining bytes afterwards.
    pub fn from_bytes(bytes: &[u8]) -> Result<Header, HeaderError> {
        let (identifier, bytes) = split_const::<8>(bytes)
            .ok_or(HeaderError::MissingIdentifier)?;

        if *identifier != IDENTIFIER {
            return Err(HeaderError::BadIdentifier);
        }

        let (main, rest) = split_as_u32(bytes)
            .ok_or(HeaderError::MissingMain)?;

        if *rest != [] {
            return Err(HeaderError::TooManyBytes);
        }

        let header = Self {
            identifier: *identifier,
            main
        };
        Ok(header)
    }
}
