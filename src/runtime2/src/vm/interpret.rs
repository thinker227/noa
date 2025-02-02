use crate::exception::Exception;
use crate::opcode;

use super::Vm;

impl Vm {
    /// Runs the interpreter indefinitely until an exception occurs, or the call stack runs out.
    fn _run(&mut self) -> Result<(), Exception> {
        while !self.call_stack.is_empty() {
            self._interpret_instruction()?;
        }

        Ok(())
    }

    /// Interprets the current instruction pointed to by [`Self::ip`].
    fn _interpret_instruction(&mut self) -> Result<(), Exception> {
        let address = self.ip;
        let opcode = self.code.get(address)
            .ok_or(Exception::Overrun)?;

        match *opcode {
            opcode::NO_OP => {},

            opcode::JUMP => todo!(),

            opcode::JUMP_IF => todo!(),

            opcode::CALL => todo!(),

            opcode::RET => todo!(),

            opcode::ENTER_TEMP_FRAME => todo!(),

            opcode::EXIT_TEMP_FRAME => todo!(),

            opcode::PUSH_INT => todo!(),

            opcode::PUSH_BOOL => todo!(),

            opcode::PUSH_FUNC => todo!(),

            opcode::PUSH_NIL => todo!(),

            opcode::POP => todo!(),

            opcode::DUP => todo!(),

            opcode::SWAP => todo!(),

            opcode::STORE_VAR => todo!(),

            opcode::LOAD_VAR => todo!(),

            opcode::ADD => todo!(),

            opcode::SUB => todo!(),

            opcode::MULT => todo!(),

            opcode::DIV => todo!(),

            opcode::EQUAL => todo!(),

            opcode::LESS_THAN => todo!(),

            opcode::NOT => todo!(),

            opcode::AND => todo!(),

            opcode::OR => todo!(),

            opcode::GREATER_THAN => todo!(),

            opcode::BOUNDARY => return Err(Exception::Overrun),

            _ => return Err(Exception::UnknownOpcode(*opcode))
        }

        self.ip += 1;

        Ok(())
    }
}
