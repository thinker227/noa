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
    allocated: usize,
}

/// The memory kept track of by a [Gc].
#[derive(Debug)]
struct Memory {
    /// The root object of the memory. There's nothing special about this object,
    /// it just happens to be the first one allocated.
    root: *mut Obj,

    /// The latest object allocated by the GC. This object always has its [Obj::next] field set to [None]
    /// because it's the last object in the chain of allocated objects.
    current: *mut Obj,
}

/// An iterator over a [Memory].
struct MemoryIterator {
    /// The next value to iterate.
    next: Option<*mut Obj>,
}

/// The color of a visited object.
#[derive(Debug, PartialEq, Eq, Clone, Copy)]
enum Color {
    White,
    Gray,
    Black,
}

/// A value wrapped in an object managed by a [Gc].
/// 
/// An [Obj] can be dereferenced as a [Value], and for all intents and purposes *is* a value
/// (which just happens to be managed by a GC).
/// 
/// Cloning or copying an [Obj] will end up with the cloned [Obj] not being tracked by a GC,
/// so [clone_object] exists to properly clone an [Obj].
#[derive(Debug, PartialEq, Eq)]
pub struct Obj {
    /// The color the object is marked in.
    color: Color,

    /// The next object in the chain of tracked objects.
    /// 
    /// This effectively forms a linked list of objects kept in memory.
    next: Option<*mut Obj>,

    /// The inner value of the object.
    value: Value,
}

/// A [Layout] for an [Obj].
const OBJ_LAYOUT: Layout = Layout::new::<Obj>();

/// Allocates an [Obj] in memory.
unsafe fn allocate_obj(obj: Obj) -> *mut Obj {
    let ptr = alloc::alloc(OBJ_LAYOUT) as *mut Obj;
    *ptr = obj;

    ptr
}

/// Creates an [Obj] around a [Value] and allocates it in memory.
unsafe fn create_obj(value: Value) -> *mut Obj {
    let obj = Obj {
        color: Color::White,
        next: None,
        value
    };

    allocate_obj(obj)
}

/// Frees an [Obj] from memory.
unsafe fn free_obj(obj: *mut Obj) {
    alloc::dealloc(obj as *mut u8, OBJ_LAYOUT);
}

/// Clones an [Obj], producing a shallow copy of it.
/// Returns a mutable reference to the newly cloned [Obj].
/// 
/// This function allocates the cloned [Obj] within the GC which owns it,
/// which is why it takes a mutable reference to the [Obj].
pub fn clone_object<'a>(obj: &'a mut Obj) -> &'a mut Obj {
    // The new object is "inserted" immediately before the existing object in the
    // chain of allocated objects. This is enough to "allocate" the new object within
    // the GC, since it's now placed inside the chain which the GC uses when sweeping.

    let new = Obj {
        color: obj.color,
        next: obj.next,
        value: obj.value
    };

    let ptr = unsafe {
        allocate_obj(new)
    };
    obj.next = Some(ptr);

    unsafe {
        &mut *ptr
    }
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
    pub fn allocate<'a>(&'a mut self, value: Value) -> &'a mut Obj {
        unsafe {
            let obj = create_obj(value);

            self.attach_to_memory(obj);

            &mut *obj
        }
    }

    /// Attaches a pointer to an [Obj] to the GC's memory.
    unsafe fn attach_to_memory(&mut self, obj: *mut Obj) {
        match &mut self.memory {
            Some(mem) => {
                // Memory has previously been allocated,
                // so we just have to update the next pointer in the current object.
                (*mem.current).next = Some(obj);
            },
            None => {
                // No memory has previously been allocated,
                // so we have to set up the memory with the current object.
                self.memory = Some(Memory {
                    root: obj,
                    current: obj
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

        let mut previous: Option<*mut Obj> = None;

        for obj in mem.iter() {
            let color = (*obj).color;

            if color != Color::White {
                // Revert the object back to being marked as white
                // so that the object is ready for the next collection.
                (*obj).color = Color::White;

                previous = Some(obj);

                continue;
            }

            // The object is marked as white and is unreachable. Free it.

            // If there is a previous object in the chain, we have to update
            // its next pointer to the next object of the current object.
            if let Some(prev) = previous {
                (*prev).next = (*obj).next;
            }

            free_obj(obj);
        }
    }
}

impl Memory {
    /// Creates an iterator over the memory.
    pub fn iter(&self) -> MemoryIterator {
        MemoryIterator {
            next: Some(self.root)
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
    type Item = *mut Obj;

    fn next(&mut self) -> Option<Self::Item> {
        let obj = match self.next {
            Some(obj) => obj,
            None => return None,
        };

        unsafe {
            self.next = (*obj).next;
        }

        Some(obj)
    }
}

impl Deref for Obj {
    type Target = Value;

    fn deref(&self) -> &Self::Target {
        &self.value
    }
}

impl DerefMut for Obj {
    fn deref_mut(&mut self) -> &mut Self::Target {
        &mut self.value
    }
}
