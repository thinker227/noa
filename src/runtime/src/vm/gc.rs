use std::ops::{Deref, DerefMut};
use std::alloc::{self, Layout};

use crate::runtime::value::Value;

/// A garbage collector which allocates and manages memory.
/// 
/// A [Gc] owns the objects it allocates, despite this not necessarily being kept track of by the borrow checker.
/// Once the [Gc] is dropped, all the objects it has allocated are freed.
#[derive(Debug)]
pub struct Gc {
    /// The GC's memory. This is [None] if there is no memory allocated yet.
    memory: Option<Memory>,

    /// The amount of allocated objects.
    /// This is not correlated to the amount of bytes allocated.
    allocated: usize,
}

/// The memory kept track of by a [Gc].
#[derive(Debug)]
struct Memory {
    /// The head object of the memory.
    /// This is the latest allocated object and the head of the chain of allocated object.
    head: *mut dyn Managed,
}

/// An iterator over a [Memory].
struct MemoryIterator {
    /// The next value to iterate.
    next: Option<*mut dyn Managed>,
}

/// The color of a visited object.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
enum Color {
    White,
    Gray,
    Black,
}

/// Contains data required by a [Gc] to track a managed object.
#[derive(Debug, PartialEq, Eq)]
pub struct Obj {
    /// The color the object is marked in.
    color: Color,

    /// The previous object in the chain of tracked objects.
    previous: Option<*mut dyn Managed>,
}

/// Trait for GC-managed types.
pub trait Managed {
    /// Gets the [Obj] for this managed instance.
    fn obj(&mut self) -> &mut Obj;
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
            memory: None,
            allocated: 0
        }
    }

    /// Allocates a new object containing a managed value.
    /// Memory will be allocated specifically for the type `T`.
    /// 
    /// This function takes another function which creates the managed object using
    /// an [Obj] which is used to track the object within the [Gc].
    /// This tracking object should be stored somewhere in the type `T` as it is
    /// a unique marker for that specific instance of the managed object.
    /// 
    /// The returned [GcRef] has the same lifetime as the [Gc] which allocated it.
    /// It is *extremely* unsafe to dereference the [GcRef] after the [Gc] has been dropped
    /// and the allocated memory of the reference freed.
    pub fn allocate<T: Managed + 'static>(& mut self, create: impl FnOnce(Obj) -> T) -> GcRef {
        let header = Obj {
            color: Color::White,
            previous: self.memory
                .as_ref()
                .map(|mem| mem.head)
        };

        let value = create(header);

        unsafe {
            let obj = allocate_obj(value);

            self.attach_to_memory(obj);

            GcRef {
                ptr: obj
            }
        }
    }

    /// Attaches a pointer to a managed object to the GC's memory.
    unsafe fn attach_to_memory(&mut self, obj: *mut dyn Managed) {
        match &mut self.memory {
            Some(mem) => {
                // Memory has previously been allocated,
                // so we just have to update the head to the new object.
                mem.head = obj;
            },
            None => {
                // No memory has previously been allocated,
                // so we have to set up the memory with the current object.
                self.memory = Some(Memory {
                    head: obj
                });
            }
        };

        self.allocated += 1;
    }

    /// Runs a collection and frees any unused allocated memory.
    /// 
    /// Since all memory which can't be reached from the stack is by definition unreachable,
    /// this function takes a reference to the stack of the VM to know what to collect.
    pub fn collect(&mut self, stack: &Vec<Value>) {
        Self::mark_objects(stack);
        
        unsafe {
            self.sweep();
        }
    }

    fn mark_objects(values: &Vec<Value>) {
        // There aren't any values yet which store pointers, so this doesn't do anything lmao.
    }

    unsafe fn sweep(&mut self) {
        let mem = match &mut self.memory {
            Some(mem) => mem,
            None => return,
        };

        let mut last: Option<*mut dyn Managed> = None;

        for obj in mem.iter() {
            let color = (*obj).obj().color;

            if color != Color::White {
                // Revert the object back to being marked as white
                // so that the object is ready for the next collection.
                (*obj).obj().color = Color::White;

                last = Some(obj);

                continue;
            }

            // The object is marked as white and is unreachable. Free it.

            // If there is a last object in the chain, we have to update
            // its previous pointer to the previous of the current object.
            if let Some(prev) = last {
                (*prev).obj().previous = (*obj).obj().previous;
            }

            free_obj(obj);

            self.allocated -= 1;
        }
    }
}

impl Memory {
    /// Creates an iterator over the memory.
    pub fn iter(&self) -> MemoryIterator {
        MemoryIterator {
            next: Some(self.head)
        }
    }
}

impl Drop for Memory {
    fn drop(&mut self) {
        for obj in self.iter() {
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
            self.next = (*obj).obj().previous;
        }

        Some(obj)
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
