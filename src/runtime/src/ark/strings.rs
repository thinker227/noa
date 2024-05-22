use std::string::FromUtf8Error;

use crate::utility::bytes::{split, split_as_u32};

/// Represents a string section.
#[derive(Debug, PartialEq, Eq)]
pub struct StringSection {
    pub strings_length: u32,
    pub strings: Vec<String>,
}

/// An error from reading an invalid string section.
#[derive(Debug, PartialEq, Eq)]
pub enum StringSectionError {
    MissingLength,
    IncongruentLength,
    StringError(StringError)
}

impl StringSection {
    /// Attempts to construct a string section from a sequence of bytes.
    /// Reads only the amount of bytes specified by the section itself
    /// and returns the rest.
    pub fn from_bytes(bytes: &[u8]) -> Result<(StringSection, &[u8]), StringSectionError> {
        let (strings_length, bytes) = split_as_u32(bytes)
            .ok_or(StringSectionError::MissingLength)?;

        let (strings_bytes, rest) = split(bytes, strings_length as usize)
            .ok_or(StringSectionError::IncongruentLength)?;

        let strings = parse_strings(strings_bytes)
            .map_err(|e| StringSectionError::StringError(e))?;

        let section = Self {
            strings_length,
            strings
        };
        Ok((section, rest))
    }
}

fn parse_strings(mut bytes: &[u8]) -> Result<Vec<String>, StringError> {
    let mut strings = Vec::new();

    while !bytes.is_empty() {
        let (string, rest) = parse_string(bytes)?;
        strings.push(string);
        bytes = rest;
    }

    Ok(strings)
}

/// An error from reading an invalid string.
#[derive(Debug, PartialEq, Eq)]
pub enum StringError {
    MissingLength,
    IncongruentLength,
    Utf8Error(FromUtf8Error),
}

fn parse_string(bytes: &[u8]) -> Result<(String, &[u8]), StringError> {
    let (length, bytes) = split_as_u32(bytes)
        .ok_or(StringError::MissingLength)?;

    let (bytes, rest) = split(bytes, length as usize)
        .ok_or(StringError::IncongruentLength)?;

    let string = String::from_utf8(bytes.into())
        .map_err(|e| StringError::Utf8Error(e))?;

    Ok((string, rest))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn reads_string_section() {
        let bytes = &[
            0, 0, 0, 25,
            0, 0, 0, 3,
            117, 119, 117,
            0, 0, 0, 14,
            111, 32, 105, 32, 195, 165, 97, 32, 195, 164, 101, 32, 195, 182
        ];

        let (section, rest) = StringSection::from_bytes(bytes).unwrap();

        assert_eq!(section, StringSection {
            strings_length: 25,
            strings: vec![
                "uwu".into(),
                "o i åa äe ö".into()
            ]
        });
        assert_eq!(rest, []);
    }
}
