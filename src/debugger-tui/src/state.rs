pub struct State {
    pub focus: Focus,
}

pub enum Focus {
    Stack,
    ExecInfo,
    CallStack,
}

impl Default for State {
    fn default() -> Self {
        Self {
            focus: Focus::ExecInfo,
        }
    }
}
