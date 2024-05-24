use std::fmt::Debug;
use crate::vm::gc::{GcTracker, Managed, Spy, Trace};

// I really wish there was a more elegant way to be able to match on the type of an
// object stored in a `dyn Object`, but afaik this is probably the most painless way.
// It really bugs me that it requires two almost identical enums tho.

/// Contains a reference to an [Object] distinguished as an enum.
/// 
/// This exists as a workaround for the lack of runtime type info within trait objects,
/// so that it's possible to tell what kind of object a `dyn Object` is.
pub enum ObjectRef<'a> {
    String(&'a StringObject),
}

/// Same as [ObjectRef] but contains a mutable reference.
pub enum ObjectRefMut<'a> {
    String(&'a mut StringObject),
}

/// Trait for heap-allocated objects representing values.
pub trait Object: Managed + Debug {
    /// Gets an [ObjectRef] containing a reference to [self].
    fn get_ref(&self) -> ObjectRef;

    /// Gets an [ObjectRefMut] containing a mutable reference to [self].
    fn get_ref_mut(&mut self) -> ObjectRefMut;
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

impl Object for StringObject {
    fn get_ref(&self) -> ObjectRef {
        ObjectRef::String(self)
    }

    fn get_ref_mut(&mut self) -> ObjectRefMut {
        ObjectRefMut::String(self)
    }
}
