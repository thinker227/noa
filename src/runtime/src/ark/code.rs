use crate::utility::bytes::{split, split_as_u32};

/// Represents a code section.
#[derive(Debug, PartialEq, Eq)]
pub struct CodeSection {
    pub code_length: u32,
    pub code: Vec<u8>,
}

/// An error from reading an invalid code section.
#[derive(Debug, PartialEq, Eq)]
pub enum CodeSectionError {
    MissingLength,
    IncongruentLength,
}

impl CodeSection {
    /// Attempts to construct a code section from a sequence of bytes.
    /// Reads only the amount of bytes specified by the section itself
    /// and returns the rest.
    pub fn from_bytes(bytes: &[u8]) -> Result<(CodeSection, &[u8]), CodeSectionError> {
        let (code_length, bytes) = split_as_u32(bytes)
            .ok_or(CodeSectionError::MissingLength)?;

        let (code_bytes, rest) = split(bytes, code_length as usize)
            .ok_or(CodeSectionError::IncongruentLength)?;

        let code = code_bytes.to_vec();

        let section = CodeSection {
            code_length,
            code
        };

        Ok((section, rest))
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn reads_code_section() {
        let bytes = &[
            0, 0, 0, 4,
            0xe, 6, 2, 1
        ];

        let (section, rest) = CodeSection::from_bytes(bytes).unwrap();

        assert_eq!(section, CodeSection {
            code_length: 4,
            code: vec![
                0xe, 6, 2, 1
            ]
        });
        assert_eq!(rest, []);
    }

    #[test]
    fn missing_length() {
        let bytes = &[
            6, 9
        ];

        let e = CodeSection::from_bytes(bytes).unwrap_err();

        assert_eq!(e, CodeSectionError::MissingLength);
    }

    #[test]
    fn incongruent_length() {
        let bytes = &[
            0, 0, 0, 4,
            6, 9
        ];

        let e = CodeSection::from_bytes(bytes).unwrap_err();

        assert_eq!(e, CodeSectionError::IncongruentLength);
    }
}
