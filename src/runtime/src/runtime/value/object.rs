use std::fmt::Debug;
use crate::vm::gc::{GcTracker, Managed, Spy, Trace};

/// Trait for heap-allocated objects representing values.
pub trait Object: Managed + Debug {
    // todo
}

// wip

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

impl Object for StringObject {}
