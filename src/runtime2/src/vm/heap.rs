use std::collections::HashMap;
use std::ptr;

use crate::value::Value;

/// An address to data on a [`Heap`].
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct HeapAddress(pub usize);

/// A 'slot' of memory on a [`Heap`].
/// 
/// A slot can be either free - containing an address to the next free slot on the heap,
/// or be filled - containing the heap data allocated in the slot.
#[derive(Debug)]
enum MemorySlot {
    Free(Free),
    Filled(HeapData),
}

impl MemorySlot {
    /// Returns a reference to the current slot as a free slot.
    /// Panics if the slot isn't free.
    pub fn as_free(&self) -> &Free {
        match self {
            MemorySlot::Free(free) => free,
            MemorySlot::Filled(_) => panic!("Memory slot isn't free."),
        }
    }

    /// Returns a mutable reference the current slot as a free slot.
    /// Panics if the slot isn't free.
    pub fn as_free_mut(&mut self) -> &mut Free {
        match self {
            MemorySlot::Free(free) => free,
            MemorySlot::Filled(_) => panic!("Memory slot isn't free."),
        }
    }
}

/// Data about a free memory slot.
#[derive(Debug)]
struct Free {
    next_free: Option<usize>
}

/// Data stored on a [`Heap`] and containing additional metadata.
#[derive(Debug)]
struct HeapData {
    marked: bool,
    value: HeapValue,
}

/// A value allocated on a [`Heap`].
#[derive(Debug)]
pub enum HeapValue {
    String(String),
    List(Vec<Value>),
    Object(HashMap<String, Value>),
}

/// A memory heap for managing heap-allocated data and garbage collection of that data.
#[derive(Debug)]
pub struct Heap {
    mem: Vec<MemorySlot>,
    used: usize,
    first_free: Option<usize>,
}

/// An error produced by [`Heap::get`] and [`Heap::get_mut`].
#[derive(Debug, PartialEq, Eq)]
pub enum HeapGetError {
    /// The specified address was out of bounds of the heap.
    OutOfBounds,
    /// The slot at the specified address is free and doesn't contain any data.
    SlotFreed,
}

/// An error produced by [`Heap::alloc`].
#[derive(Debug, PartialEq, Eq)]
pub enum HeapAllocError {
    /// The heap is out of available memory.
    OutOfNoMemory,
}

impl Heap {
    /// Creates a new [`Heap`] with a specified size worth of individual pieces of data able to be allocated.
    pub fn new(size: usize) -> Self {
        // let mem = Vec::with_capacity(size);
        let mut mem = Vec::new();
        let mut i = 0;
        mem.resize_with(size, || {
            i += 1;
            let next_free = if i == size { None } else { Some(i) };
            MemorySlot::Free(Free { next_free })
        });

        Self {
            mem,
            used: 0,
            first_free: Some(0),
        }
    }

    /// Gets a reference to a value at a specified address on the heap.
    pub fn get(&self, address: HeapAddress) -> Result<&HeapValue, HeapGetError> {
        match self.mem.get(address.0) {
            Some(MemorySlot::Filled(data)) => Ok(&data.value),
            Some(MemorySlot::Free(_)) => Err(HeapGetError::SlotFreed),
            None => Err(HeapGetError::OutOfBounds),
        }
    }
    
    /// Gets a mutable reference to a value at a specified address on the heap.
    pub fn get_mut(&mut self, address: HeapAddress) -> Result<&mut HeapValue, HeapGetError> {
        match self.mem.get_mut(address.0) {
            Some(MemorySlot::Filled(data)) => Ok(&mut data.value),
            Some(MemorySlot::Free(_)) => Err(HeapGetError::SlotFreed),
            None => Err(HeapGetError::OutOfBounds),
        }
    }

    /// Allocates a value on the heap.
    pub fn alloc(&mut self, value: HeapValue) -> Result<(), HeapAllocError> {
        // First just check whether there even is memory left to allocate at.
        let address = match self.first_free {
            Some(x) => x,
            None => return Err(HeapAllocError::OutOfNoMemory), // no more memory
        };

        let slot = &mut self.mem[address];

        // Update the first free address to be the first free slot after
        // the slot we're currently gonna allocate at.
        self.first_free = slot.as_free().next_free;

        // Now! We allocate!
        *slot = MemorySlot::Filled(HeapData { value, marked: false });
        self.used += 1;

        Ok(())
    }

    /// Runs a garbage collection, freeing any unreferenced data.
    /// 
    /// Takes a vector to a stack of values to search for references to heap-allocated data.
    /// Any data not referenced directly or indirectly by a value in the vector
    /// will be freed.
    pub fn collect(&mut self, referenced: &Vec<Value>) {
        self.mark(referenced);
        self.core_collect();
    }

    /// Marks all heap-allocated data referenced directly or indirectly by a value.
    fn mark(&mut self, referenced: &Vec<Value>) {
        // This is just simple depth-first graph traversal.

        let mut to_visit: Vec<usize> = Self::extact_references(referenced.iter()).collect();

        while let Some(address) = to_visit.pop() {
            let data = match self.mem.get_mut(address) {
                Some(MemorySlot::Filled(data)) => data,
                _ => continue
            };

            // If the data has already been marked, that means we've already visited it
            // and its contained references.
            if data.marked {
                return;
            }

            data.marked = true;

            // Add the contained references to the list of addresses to visit.
            match &data.value  {
                HeapValue::String(_) => {},
                HeapValue::List(xs) => {
                    let addresses = Self::extact_references(xs.iter());
                    to_visit.extend(addresses);
                },
                HeapValue::Object(map) => {
                    let addresses = Self::extact_references(map.values());
                    to_visit.extend(addresses);
                },
            }
        }
    }

    /// Extracts referenced heap addresses from an iterator of values.
    fn extact_references<'a>(values: impl Iterator<Item = &'a Value> + 'a) -> impl Iterator<Item = usize> + 'a {
        values.filter_map(|x| match x {
            Value::Object(adr) => Some(adr.0),
            _ => None
        })
    }

    /// Core implementation of a run of garbage collection.
    fn core_collect(&mut self) {
        let is_full = self.is_full();

        // Get a reference to the space on the heap currently being used.
        let used = &mut self.mem[..self.used];

        // A raw pointer to the `next_free` field of the last encountered free memory slot.
        // Starts off as None, because at the start, no last free memory slot has been encountered yet.
        let mut last_free: Option<*mut Option<usize>> = None;
        
        for address in 0..self.used {
            let slot = &mut used[address];

            match slot {
                MemorySlot::Free(Free { next_free }) => {
                    // We found free memory! Place a pointer to its `next_free` field into `last_free`
                    // so we can later update it once we encounter newly freed memory.
                    last_free = Some(ptr::from_mut(next_free));
                },
                MemorySlot::Filled(HeapData { marked, .. }) if *marked => {
                    // Reset this slot to be unmarked for the next run.
                    *marked = false;
                },
                MemorySlot::Filled(HeapData { marked: false, .. }) => {
                    if let Some(last_free_next_free) = last_free {
                        // Get the next free address pointed to by the last free slot.
                        let next_free = unsafe {
                            *last_free_next_free
                        };

                        // Free and set the current slot.
                        let r = &mut used[address];
                        *r = MemorySlot::Free(Free { next_free });

                        // Update the `next_free` field of the last free slot to be the current address.
                        unsafe {
                            *last_free_next_free = Some(address);
                        }

                        // Finally, update `last_free` to be the `next_free` field of the current slot.
                        last_free = Some(&mut r.as_free_mut().next_free);
                    } else {
                        // There is no last encountered free memory slot,
                        // which means that all slots before this are filled,
                        // meaning that this is the new first free address.
                        self.first_free = Some(address);

                        // If the heap is full then there is no more memory, so the next free memory is None.
                        // Otherwise, it is the next slot after the space of currently used memory.
                        let next_free = if is_full {
                            None
                        } else {
                            Some(self.used)
                        };

                        // Free and set the current slot.
                        let r = &mut used[address];
                        *r = MemorySlot::Free(Free { next_free });

                        // Finally, update `last_free` to be the `next_free` field of the current slot.
                        last_free = Some(&mut r.as_free_mut().next_free);
                    }

                    // Decrease the amount of used memory.
                    self.used -= 1;
                },
                _ => {}
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::assert_matches::assert_matches;

    #[test]
    fn new_initializes_data() {
        let heap = Heap::new(3);

        assert_matches!(&heap.mem[..], [
            MemorySlot::Free(Free { next_free: Some(1) }),
            MemorySlot::Free(Free { next_free: Some(2) }),
            MemorySlot::Free(Free { next_free: None })
        ]);
        assert_eq!(heap.used, 0);
        assert_eq!(heap.first_free, Some(0));
    }

    #[test]
    fn allocate_allocates_object() {
        let mut heap = Heap::new(3);
        
        heap.alloc(HeapValue::String("uwu".into())).unwrap();

        assert_matches!(&heap.mem[..], [
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(s),
                ..
            }),
            MemorySlot::Free(Free { next_free: Some(2) }),
            MemorySlot::Free(Free { next_free: None })
        ] if s == "uwu");

        assert_eq!(heap.used, 1);
        assert_eq!(heap.first_free, Some(1));
    }

    #[test]
    fn allocate_allocates_until_end() {
        let mut heap = Heap::new(3);
        
        heap.alloc(HeapValue::String("uwu".into())).unwrap();
        heap.alloc(HeapValue::String("owo".into())).unwrap();
        heap.alloc(HeapValue::String("^w^".into())).unwrap();
        
        assert_eq!(
            heap.alloc(HeapValue::String(";w;".into())),
            Err(HeapAllocError::OutOfNoMemory)
        );

        assert_matches!(&heap.mem[..], [
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(a),
                ..
            }),
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(b),
                ..
            }),
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(c),
                ..
            })
        ] if a == "uwu" && b == "owo" && c == "^w^");

        assert_eq!(heap.used, 3);
        assert_eq!(heap.first_free, None);
    }

    #[test]
    fn get_returns_references_to_allocated_data() {
        let mut heap = Heap::new(2);

        heap.alloc(HeapValue::String("uwu".into())).unwrap();

        let val = heap.get(HeapAddress(0)).unwrap();
        assert_matches!(
            val,
            HeapValue::String(s) if s == "uwu"
        );

        let val = heap.get(HeapAddress(1));
        assert_matches!(val, Err(HeapGetError::SlotFreed));

        let val = heap.get(HeapAddress(2));
        assert_matches!(val, Err(HeapGetError::OutOfBounds));
    }
}
