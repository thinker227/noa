use crate::vm::gc::{GcTracker, Managed, Spy, Trace};

/// An enum which contains a reference to a variant of a managed object.
pub enum Object<'a> {
    String(&'a StringObject),
}

/// An enum which contains a mutable reference to a variant of a managed object.
pub enum ObjectMut<'a> {
    String(&'a mut StringObject),
}

/// An object representing a string.
#[derive(Debug)]
pub struct StringObject {
    tracker: GcTracker,
    string: String,
}

impl Trace for StringObject {
    fn trace(&mut self, spy: &Spy) {}
}

impl Managed for StringObject {
    fn tracker(&mut self) -> &mut GcTracker {
        &mut self.tracker
    }
}
