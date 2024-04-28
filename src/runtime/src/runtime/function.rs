use super::opcode::{FuncId, Opcode};

/// A function executed by the runtime.
#[derive(Debug)]
pub struct Function {
    id: FuncId,
    code: Vec<Opcode>
}

impl Function {
    /// Gets the ID of the function.
    pub fn id(&self) -> FuncId {
        self.id
    }

    /// Gets the bytecode of the function.
    pub fn code(&self) -> &Vec<Opcode> {
        &self.code
    }
}
