use std::collections::HashMap;
use std::ptr;

use crate::value::{Closure, Value};

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
    /// The raw memory within the heap.
    mem: Vec<MemorySlot>,
    /// The size (from the start of memory) of the block of memory which currently contains filled slots.
    used: usize,
    /// An index into memory which marks the first free slot from the start of memory.
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

    /// Returns whether the heap is currently full and no more memory can be allocated.
    pub fn is_full(&self) -> bool {
        // Should be impossible for there to be more used memory than there is available,
        // but using >= just to be safe.
        self.used >= self.mem.len()
    }

    /// Allocates a value on the heap.
    pub fn alloc(&mut self, value: HeapValue) -> Result<HeapAddress, HeapAllocError> {
        // First just check whether there even is memory left to allocate at.
        let address = match self.first_free {
            Some(x) => x,
            None => return Err(HeapAllocError::OutOfNoMemory), // no more memory
        };

        let slot = &mut self.mem[address];

        // Update the first free address to be the first free slot after
        // the slot we're currently gonna allocate at.
        self.first_free = slot.as_free().next_free;

        // Update the size of the block of used memory only if the current address is outside it.
        // Otherwise, we're allocating at some address which is already inside the block of used memory,
        // so we don't need to update its size.
        if address >= self.used {
            self.used = address + 1;
        }

        // Now! We allocate!
        *slot = MemorySlot::Filled(HeapData { value, marked: false });

        Ok(HeapAddress(address))
    }

    /// Runs a garbage collection, freeing any unreferenced data.
    /// 
    /// Takes a reference to a vector of values to search for references to heap-allocated data.
    /// Any data not referenced directly or indirectly by a value in the vector will be freed.
    pub fn collect(&mut self, referenced: &Vec<Value>) {
        self.mark(referenced);
        self.core_collect();
    }

    /// Marks all heap-allocated data referenced directly or indirectly by a value.
    fn mark(&mut self, referenced: &Vec<Value>) {
        // This is just simple depth-first graph traversal.

        let mut to_visit: Vec<usize> = Self::extract_references(referenced.iter()).collect();

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
                    let addresses = Self::extract_references(xs.iter());
                    to_visit.extend(addresses);
                },
                HeapValue::Object(map) => {
                    let addresses = Self::extract_references(map.values());
                    to_visit.extend(addresses);
                },
            }
        }
    }

    /// Extracts referenced heap addresses from an iterator of values.
    fn extract_references<'a>(values: impl Iterator<Item = &'a Value> + 'a) -> impl Iterator<Item = usize> + 'a {
        values.filter_map(|x| match x {
            Value::Object(adr) => Some(adr.0),
            Value::Function(Closure { captures: Some(captures), .. }) => Some(captures.0),
            _ => None
        })
    }

    /// Core implementation of a run of garbage collection.
    fn core_collect(&mut self) {
        let is_full = self.is_full();

        // Tracks the new size of the block of used memory.
        let mut new_used_size = 0;

        // A raw pointer to the `next_free` field of the last encountered free memory slot.
        // Starts off as None, because at the start, no last free memory slot has been encountered yet.
        let mut last_free: Option<*mut Option<usize>> = None;

        // Get a reference to the space on the heap currently being used.
        let used = &mut self.mem[..self.used];
        
        for address in 0..self.used {
            let slot = &mut used[address];

            match slot {
                MemorySlot::Free(Free { next_free }) => {
                    // We found free memory! Place a pointer to its `next_free` field into `last_free`
                    // so we can later update it once we encounter newly freed memory.
                    last_free = Some(ptr::from_mut(next_free));
                },
                MemorySlot::Filled(HeapData { marked, .. }) if *marked => {
                    // This slot is actively used memory.
                    
                    // Reset this slot to be unmarked for the next run.
                    *marked = false;

                    // Update the new size of the block of used memory.
                    new_used_size = address + 1;
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
                },
                _ => {}
            }
        }

        // Lastly, update the size of the block of used memory.
        self.used = new_used_size;
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::assert_matches::assert_matches;

    fn alloc(heap: &mut Heap, value: HeapValue, expected_address: usize) -> HeapAddress {
        let address = heap.alloc(value);

        assert_eq!(address, Ok(HeapAddress(expected_address)));

        address.unwrap()
    }

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
        
        alloc(&mut heap, HeapValue::String("uwu".into()), 0);

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
        
        alloc(&mut heap, HeapValue::String("uwu".into()), 0);
        alloc(&mut heap, HeapValue::String("owo".into()), 1);
        alloc(&mut heap, HeapValue::String("^w^".into()), 2);
        
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

        alloc(&mut heap, HeapValue::String("uwu".into()), 0);

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

    #[test]
    fn collect_collects_unreferenced_data() {
        let mut heap = Heap::new(2);

        alloc(&mut heap, HeapValue::String("uwu".into()), 0);
        alloc(&mut heap, HeapValue::String("owo".into()), 1);

        heap.collect(&vec![]);

        assert_eq!(heap.used, 0);
        assert_eq!(heap.first_free, Some(0));

        assert_matches!(heap.mem[..], [
            MemorySlot::Free(Free { next_free: Some(1) }),
            MemorySlot::Free(Free { next_free: None })
        ]);
    }

    #[test]
    fn collect_sets_next_free_to_next_unused_free_slot() {
        let mut heap = Heap::new(2);

        alloc(&mut heap, HeapValue::String("uwu".into()), 0);

        heap.collect(&vec![]);

        assert_eq!(heap.used, 0);
        assert_eq!(heap.first_free, Some(0));

        assert_matches!(heap.mem[..], [
            MemorySlot::Free(Free { next_free: Some(1) }),
            MemorySlot::Free(Free { next_free: None })
        ]);
    }

    #[test]
    fn collect_handles_non_contiguous_blocks() {
        let mut heap = Heap::new(3);
        
        alloc(&mut heap, HeapValue::String("uwu".into()), 0);
        alloc(&mut heap, HeapValue::String("owo".into()), 1);
        alloc(&mut heap, HeapValue::String("^w^".into()), 2);

        heap.collect(&vec![
            Value::Object(HeapAddress(0)),
            Value::Object(HeapAddress(2))
        ]);

        assert_eq!(heap.used, 3);
        assert_eq!(heap.first_free, Some(1));

        assert_matches!(heap.mem[..], [
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(..),
                ..
            }),
            MemorySlot::Free(Free { next_free: None }),
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(..),
                ..
            }),
        ]);
    }

    #[test]
    fn collect_marks_references_through_objects() {
        let mut heap = Heap::new(4);
        
        alloc(&mut heap, HeapValue::List(vec![
            Value::Object(HeapAddress(1))
        ]), 0);
        alloc(&mut heap, HeapValue::List(vec![
            Value::Object(HeapAddress(3))
        ]), 1);
        alloc(&mut heap, HeapValue::String("uwu".into()), 2);
        alloc(&mut heap, HeapValue::String("owo".into()), 3);

        heap.collect(&vec![
            Value::Object(HeapAddress(0))
        ]);

        assert_eq!(heap.used, 4);
        assert_eq!(heap.first_free, Some(2));

        assert_matches!(heap.mem[..], [
            MemorySlot::Filled(HeapData {
                value: HeapValue::List(..),
                ..
            }),
            MemorySlot::Filled(HeapData {
                value: HeapValue::List(..),
                ..
            }),
            MemorySlot::Free(Free { next_free: None }),
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(..),
                ..
            }),
        ]);
    }

    #[test]
    fn collect_handles_cyclic_references() {
        let mut heap = Heap::new(2);

        alloc(&mut heap, HeapValue::List(vec![
            Value::Object(HeapAddress(1))
        ]), 0);
        alloc(&mut heap, HeapValue::List(vec![
            Value::Object(HeapAddress(0))
        ]), 1);

        heap.collect(&vec![
            Value::Object(HeapAddress(0))
        ]);

        assert_eq!(heap.used, 2);
        assert_eq!(heap.first_free, None);

        assert_matches!(heap.mem[..], [
            MemorySlot::Filled(HeapData {
                value: HeapValue::List(..),
                ..
            }),
            MemorySlot::Filled(HeapData {
                value: HeapValue::List(..),
                ..
            })
        ]);
    }

    #[test]
    fn collect_collects_unreferenced_cyclic_references() {
        let mut heap = Heap::new(2);

        alloc(&mut heap, HeapValue::List(vec![
            Value::Object(HeapAddress(1))
        ]), 0);
        alloc(&mut heap, HeapValue::List(vec![
            Value::Object(HeapAddress(0))
        ]), 1);

        heap.collect(&vec![]);

        assert_eq!(heap.used, 0);
        assert_eq!(heap.first_free, Some(0));

        assert_matches!(heap.mem[..], [
            MemorySlot::Free(Free { next_free: Some(1) }),
            MemorySlot::Free(Free { next_free: None })
        ]);
    }

    #[test]
    fn allocate_reuses_previously_freed_memory() {
        let mut heap = Heap::new(2);

        alloc(&mut heap, HeapValue::String("uwu".into()), 0);
        alloc(&mut heap, HeapValue::String("owo".into()), 1);

        heap.collect(&vec![
            Value::Object(HeapAddress(1))
        ]);

        alloc(&mut heap, HeapValue::String(";w;".into()), 0);

        assert_eq!(heap.used, 2);
        assert_eq!(heap.first_free, None);

        assert_matches!(&heap.mem[..], [
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(a),
                ..
            }),
            MemorySlot::Filled(HeapData {
                value: HeapValue::String(b),
                ..
            })
        ] if a == ";w;" && b == "owo");
    }

    #[test]
    fn allocate_uses_next_free_memory() {
        let mut heap = Heap::new(3);

        alloc(&mut heap, HeapValue::String("uwu".into()), 0);
        alloc(&mut heap, HeapValue::String("owo".into()), 1);
        alloc(&mut heap, HeapValue::String("^w^".into()), 2);

        heap.collect(&vec![
            Value::Object(HeapAddress(1))
        ]);

        alloc(&mut heap, HeapValue::String(";w;".into()), 0);
        alloc(&mut heap, HeapValue::String("qwq".into()), 2);

        assert_eq!(heap.used, 3);
        assert_eq!(heap.first_free, None);

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
        ] if a == ";w;" && b == "owo" && c == "qwq");
    }
}
