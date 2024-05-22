use crate::runtime::exception::{ExceptionData, VMException};

#[derive(Debug)]
/// A wrapper around a sequence of bytes which reads the bytes as various data.
pub struct CodeReader {
    code: Vec<u8>,
    ip: usize,
}

impl CodeReader {
    /// Constructs a new [CodeReader].
    pub fn new(code: Vec<u8>) -> Self {
        Self {
            code,
            ip: 0
        }
    }

    /// Advances the reader by 1 byte.
    pub fn advance(&mut self) {
        self.ip += 1;
    }

    /// Jumps to the specified address.
    pub fn jump(&mut self, address: usize) {
        self.ip = address;
    }

    /// Gets the current instruction pointer.
    pub fn ip(&self) -> usize {
        self.ip
    }

    /// Reads a single byte and advances the reader by 1 byte.
    pub fn read_byte(&mut self) -> Result<u8, ExceptionData> {
        let byte = self.code.get(self.ip)
            .copied()
            .ok_or(ExceptionData::VM(VMException::FunctionOverrun))?;

        self.ip += 1;

        Ok(byte)
    }

    /// Reads a specified amount of bytes and advances the reader by that same amount.
    pub fn read_bytes(&mut self, count: usize) -> Result<&[u8], ExceptionData> {
        let start = self.ip;
        let end = start + count;

        if end > self.code.len() {
            return Err(ExceptionData::VM(VMException::FunctionOverrun));
        }

        self.ip += count;

        Ok(&self.code[start..end])
    }

    /// Same as [Self::read_bytes], but the amount of bytes is specified as a constant.
    pub fn read_bytes_const<const N: usize>(&mut self) -> Result<&[u8; N], ExceptionData> {
        let bytes = self.read_bytes(N)?
            .try_into()
            .unwrap();

        Ok(bytes)
    }

    /// Reads a single byte as a bool and advances the reader by 1 byte.
    pub fn read_bool(&mut self) -> Result<bool, ExceptionData> {
        match self.read_byte()? {
            0 => Ok(false),
            1 => Ok(true),
            _ => Err(ExceptionData::VM(VMException::MalformedOpcode))
        }
    }

    /// Reads a 32-bit big-endian unsigned integer and advances the reader by 4 bytes.
    pub fn read_u32(&mut self) -> Result<u32, ExceptionData> {
        let bytes = self.read_bytes_const::<4>()?;
        Ok(u32::from_be_bytes(*bytes))
    }

    /// Reads a 32-bit big-endian signed integer and advances the reader by 4 bytes.
    pub fn read_i32(&mut self) -> Result<i32, ExceptionData> {
        let bytes = self.read_bytes_const::<4>()?;
        Ok(i32::from_be_bytes(*bytes))
    }
}
