use std::ops::{Deref, DerefMut};
use std::alloc::{self, Layout};

use crate::runtime::value::object::{Object, ObjectMut};

/// A garbage collector which allocates and manages memory.
/// 
/// A [Gc] owns the objects it allocates, despite this not necessarily
/// being kept track of by the borrow checker.
/// Once the [Gc] is dropped, all the objects it has allocated are freed.
#[derive(Debug)]
pub struct Gc {
    /// The head object of the tracked memory.
    /// This is the latest allocated object and the head of the chain of allocated objects.
    memory_head: Option<*mut dyn Managed>,

    /// The amount of allocated objects.
    /// This is not correlated to the amount of bytes allocated.
    allocated: usize,
}

/// An iterator for the memory of a [Gc].
struct MemoryIterator {
    next: Option<*mut dyn Managed>,
}

/// Contains data required by a [Gc] to track a managed object.
#[derive(Debug, PartialEq, Eq)]
pub struct GcTracker {
    /// Whether the object is marked as traced.
    marked: bool,

    /// The previous object in the chain of tracked objects.
    previous: Option<*mut dyn Managed>,
}

/// Trait for GC-managed types.
/// 
/// Managed types are actively tracked by a [Gc] and may be freed once there are
/// no more references to the object. Instances of managed types should therefore be kept wisely
/// only inside structs which can be traced by the GC to prevent the data from attempting
/// to be used when freed. Most commonly, managed objects should be accessed through a [GcRef].
pub trait Managed: Trace {
    /// Gets the [GcTracker] for this managed instance.
    fn tracker(&mut self) -> &mut GcTracker;

    /// Gets the managed object as an [Object].
    fn as_object(&self) -> Object;

    /// Gets the managed object as an [ObjectMut].
    fn as_object_mut(&mut self) -> ObjectMut;
}

/// An object which traces through managed objects for references.
// Note: this is just a 0-sized object which one purpose is to contain the visit function.
// The function is kept associated with this object instead of as a free function
// to avoid the ability to mark objects outside of tracing.
// The purpose of the x field is to make this object only able to be created in this module.
pub struct Spy {
    pub(self) x: ()
}

/// Trait for types which can be traced for GC-managed references.
pub trait Trace {
    /// Traces the references to other managed objects referenced by this object.
    /// 
    /// The [Spy::visit] function should be called for each [GcRef] instance kept by this object.
    /// [Trace::trace] should continue being called for any objects which implement [Trace].
    fn trace(&mut self, spy: &Spy);
}

/// A reference to a [Gc]-managed object.
/// 
/// This is just a convenience wrapper around a `*mut dyn Managed`.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
pub struct GcRef {
    ptr: *mut dyn Managed
}

/// Allocates a managed object in memory.
unsafe fn allocate_obj<T: Managed + 'static>(value: T) -> *mut dyn Managed {
    let layout = Layout::for_value(&value);
    let ptr = alloc::alloc(layout) as *mut T;
    *ptr = value;

    ptr
}

/// Frees a managed object from memory.
unsafe fn free_obj(obj: *mut dyn Managed) {
    let layout = Layout::for_value(&obj);
    alloc::dealloc(obj as *mut u8, layout);
}

impl Gc {
    /// Constructs a new [Gc].
    pub fn new() -> Self {
        Self {
            memory_head: None,
            allocated: 0
        }
    }

    /// Iterates the allocated objects in the memory of the [Gc].
    pub(self) fn iter_memory(&self) -> MemoryIterator {
        MemoryIterator {
            next: self.memory_head
        }
    }

    /// Allocates a new object containing a managed value.
    /// Memory will be allocated specifically for the type `T`.
    /// 
    /// This function takes another function which creates the managed object using
    /// a [GcTracker] which is used to track the object within the [Gc].
    /// This tracker should be stored somewhere in the type `T` as it is
    /// a unique marker for that specific instance of the managed object.
    /// 
    /// The returned [GcRef] has the same lifetime as the [Gc] which allocated it.
    /// It is *extremely* unsafe to dereference the [GcRef] after the [Gc] has been dropped
    /// and the allocated memory of the reference freed.
    pub fn allocate<T: Managed + 'static>(& mut self, create: impl FnOnce(GcTracker) -> T) -> GcRef {
        let header = GcTracker {
            marked: false,
            previous: self.memory_head
        };

        let value = create(header);

        let obj = unsafe {
            allocate_obj(value)
        };

        self.memory_head = Some(obj);
        self.allocated += 1;

        GcRef {
            ptr: obj
        }
    }

    /// Runs a collection and frees any unused allocated memory.
    /// 
    /// Takes a mutable reference to an object which implements [Trace]
    /// which acts as the source object to begin tracing from.
    pub fn collect(&mut self, source: &mut impl Trace) {
        Self::mark_objects(source);
        
        unsafe {
            self.sweep();
        }
    }

    fn mark_objects(source: &mut impl Trace) {
        let spy = Spy {
            x: ()
        };

        source.trace(&spy);
    }

    unsafe fn sweep(&mut self) {
        let mut last: Option<*mut dyn Managed> = None;

        for obj in self.iter_memory() {
            let marked = &mut (*obj).tracker().marked;

            if *marked {
                // Revert the object back to being marked as white
                // so that the object is ready for the next collection.
                *marked = false;

                last = Some(obj);

                continue;
            }

            // The object is marked as white and is unreachable. Free it.

            // If there is a last object in the chain, we have to update
            // its previous pointer to the previous of the current object.
            if let Some(prev) = last {
                (*prev).tracker().previous = (*obj).tracker().previous;
            }

            free_obj(obj);

            self.allocated -= 1;
        }
    }
}

impl Drop for Gc {
    fn drop(&mut self) {
        for obj in self.iter_memory() {
            unsafe {
                free_obj(obj);
            }
        }
    }
}

impl Iterator for MemoryIterator {
    type Item = *mut dyn Managed;

    fn next(&mut self) -> Option<Self::Item> {
        let obj = match self.next {
            Some(obj) => obj,
            None => return None,
        };

        unsafe {
            self.next = (*obj).tracker().previous;
        }

        Some(obj)
    }
}

impl Spy {
    /// Visits a reference to a managed object.
    pub fn visit(&self, reference: &mut GcRef) {
        let marked = &mut reference.tracker().marked;
        
        if *marked {
            return;
        }

        *marked = true;

        reference.trace(&self);
    }
}

// Note:
// These are *extremely* unsafe if they're used after a reference
// has been freed. Still they're nice for quality of life.

impl Deref for GcRef {
    type Target = dyn Managed;

    fn deref(&self) -> &Self::Target {
        unsafe {
            &*self.ptr
        }
    }
}

impl DerefMut for GcRef {
    fn deref_mut(&mut self) -> &mut Self::Target {
        unsafe {
            &mut *self.ptr
        }
    }
}
