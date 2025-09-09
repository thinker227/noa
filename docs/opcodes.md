# Ark opcodes

This file contains an overview of the bytecode opcodes which can be used in function bodies.

Each table specifies the byte representing the opcode, the "signature" (i.e. the name and arguments) of the opcode, a description of what the opcode does, and what effect executing the opcode has on the runtime stack.

## Noop (0x0)

| Byte | Signature | Description | Stack effect |
|------|-----------|-------------|--------------|
| 0x0 | `NoOp` | No operation. | |

## Control flow (0x01-0x13)

| Byte | Signature | Description | Stack effect |
|------|-----------|-------------|--------------|
| 0x1 | `Jump <address: u32>` | Jumps to `address` within the current function. | |
| 0x2 | `JumpIf <address: u32>` | Pops the topmost value from the stack and jumps to `address` if the value is `true`. | Pops 1 value. |
| 0x3 | `Call <arg count: u32>` | Begins by looking up the element at `arg count` from the top of the stack and coerces it into a function. The remaining `arg count` elements on the top of the stack are used as arguments, and the function is called. After the function has returned, the arguments as well as the function are popped off the stack. | Cumulative: pops `arg count + 1` values. |
| 0x4 | `Ret` | Pops the topmost value from the stack, returns from the current function, then pushes the value onto the stack. | Pops 1 value. |
| 0x5 | `EnterTempFrame` | Enters a new temporary stack frame. | |
| 0x6 | `ExitTempFrame` | Exits the current temporary stack frame. Throws a runtime exception if the current stack frame is not a temporary stack frame. | |

## Stack push operations (0x14-0x31)

| Byte | Signature | Description | Stack effect |
|------|-----------|-------------|--------------|
| 0x14 | `PushFloat <val: f64>` | Pushes the 64-bit float `val` onto the stack. | Pushes 1 value. |
| 0x15 | `PushBool <val: bool>` | Pushes the boolean `val` onto the stack. | Pushes 1 value. |
| 0x16 | `PushFunc <func id: u32>` | Pushes the function with the ID `id` onto the stack, encoded as a [function ID](./ark.md#function-id). | Pushes 1 value. |
| 0x17 | `PushNil` | Pushes nil onto the stack. | Pushes 1 value. |
| 0x18 | `PushString <string index: u32>` | Pushes the string with the string index `string index` onto the stack. | Pushes 1 value. |
| 0x19 | `PushObject <dynamic: bool>` | Pushes an empty object onto the stack with `dynamic` determining whether the object is dynamic. | Pushes 1 value. |

## Miscellaneous stack operations (0x32-0x45)

| Byte | Signature | Description | Stack effect |
|------|-----------|-------------|--------------|
| 0x32 | `Pop` | Pops the topmost value from the stack and discards it. | Pops 1 value. |
| 0x33 | `Dup` | Duplicates the topmost value on the stack and pushes it to the stack. | Pushes 1 value. |
| 0x34 | `Swap` | Pops the two topmost values from the stack and pushes them back in reverse order. | Cumulative: none. |

## Locals operations (0x46-0x63)

| Byte | Signature | Description | Stack effect |
|------|-----------|-------------|--------------|
| 0x46 | `StoreVar <var index: u32>` | Pops the topmost value from the stack and stores it into a local variable with the index `var index`. | Pops 1 value. |
| 0x47 | `LoadVar <var index: u32>` | Loads the value of a local variable with the index `var index` and pushes it onto the stack. | Pushes 1 value. |

## Value operations (0x64-0xEF)

| Byte | Signature | Description | Stack effect |
|------|-----------|-------------|--------------|
| 0x64 | `Add` | Pops the two topmost values from the stack, adds them together, then pushes the result onto the stack. | Cumulative: pops 1 value. |
| 0x65 | `Sub` | Pops the two topmost values from the stack, subtracts the first value from the second, then pushes the result onto the stack. | Cumulative: pops 1 value. |
| 0x66 | `Mult` | Pops the two topmost values from the stack, multiplies them together, then pushes the result onto the stack. | Cumulative: pops 1 value. |
| 0x67 | `Div` | Pops the two topmost values from the stack, divides the second value by the first, then pushes the result onto the stack. | Cumulative: pops 1 value. |
| 0x68 | `Equal` | Pops the two topmost values from the stack, compares them for equality, then pushes the result onto the stack as a bool value. | Cumulative: pops 1 value. |
| 0x69 | `LessThan` | Pops the two topmost values on from the stack, checks whether the second value is less than the first, then pushes the result onto the stack as a bool value. | Cumulative: pops 1 value. |
| 0x6A | `Not` | Pops the topmost value from the stack, performs a logical not on it, then pushes the result onto the stack. | Cumulative: none. |
| 0x6B | `And` | Pops the two topmost values from the stack, performs a logical and on them, then pushes the result onto the stack. | Cumulative: pops 1 value. |
| 0x6C | `Or` | Pops the two topmost values from the stack, performs a logical or on them, then pushes the result onto the stack. | Cumulative: pops 1 value. |
| 0x6D | `GreaterThan` | Pops the two topmost values on from the stack, checks whether the second value is greater than the first, then pushes the result onto the stack as a bool value. | Cumulative: pops 1 value. |
| 0x6E | `Concat` |  Pops the two topmost values on from the stack, concatenates them as strings, and pushes the result onto the stack. | Cumulative: pops 1 value. |
| 0x6F | `ToString` | Pops the topmost value from the stack, coerces it into a string, then pushes the result onto the stack. | Cumulative: none. |
| 0x70 | `WriteField` | Pops the three topmost values from the stack, coerces the second into a string and the third into an object, then writes the first value as a field with the coerced string as the name into the coerced object. | Cumulative: pops 3 values. |
| 0x71 | `ReadField` | Pops the two topmost values from the stack, coerces the first into a string and the second into an object, reads a field with the coerced string as the name from the coerced object, then pushes the read value of the field onto the stack. | Cumulative: pops 1 value. |
| 0x72 | `FinalizeObject` | Pops the topmost value from the stack, ensures that is it an object without coercion, sets the object to be finalized, then pushes the object back onto the stack. | Cumulative: none. |

## Operational instructions (0xF0-0xFF)

| Byte | Signature | Description | Stack effect |
|------|-----------|-------------|--------------|
| 0xFF | `Boundary` | Specifies a boundary between two functions. Produces a runtime exception. | |
