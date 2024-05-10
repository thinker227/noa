- `NoOp` (`0`): No operation.

## Control flow (`1`-`19`)
- `Jump <address>` (`1`): Jumps to `address` within the current function.
- `JumpIf <address>` (`2`): Pops the topmost value from the stack and jumps to `address` if the value is `true`.
- `Call <arg count>` (`3`): Begins by looking up the element at `arg count` from the top of the stack and coerces it into a function. The remaining `arg count` elements are used as arguments, and the function is called.
- `Ret` (`4`): Pops the topmost value from the stack, returns from the current function, then pushes the value onto the stack.

## Stack push operations (`20`-`49`)
- `PushInt <int32>` (`20`): Pushes the 32-bit signed integer `int32` onto the stack.
- `PushBool <bool>` (`21`): Pushes the boolean `bool` onto the stack.
- `PushFunc <func id>` (`22`): Pushes the function with the ID `func id` onto the stack.
- `PushNil` (`23`): Pushes nil onto the stack.

## Miscellaneous stack operations (`50`-`69`)
- `Pop` (`50`): Pops the topmost value from the stack.
- `Dup` (`51`): Duplicates the topmost value on the stack and pushes it to the stack.
- `Swap` (`52`): Pops the two topmost values from the stack and pushes them back in reverse order.

## Locals operations (`70`-`99`)
- `StoreVar <var index>` (`70`): Pops the topmost value from the stack and stores it into a local variable with the index `var index`.
- `LoadVar <var index>` (`71`): Loads the value of a local variable with the index `var index` and pushes it onto the stack.

## Value operations (`100`-`255`)
- `Add` (`100`): Pops the two topmost values from the stack, adds them together, then pushes the result onto the stack.
- `Sub` (`101`): Pops the two topmost values from the stack, subtracts the first value from the second, then pushes the result onto the stack.
- `Mult` (`102`): Pops the two topmost values from the stack, multiplies them together, then pushes the result onto the stack.
- `Div` (`103`): Pops the two topmost values from the stack, divides the second value by the first, then pushes the result onto the stack.
- `Equal` (`104`): Pops the two topmost values from the stack, compares them for equality, then pushes the result onto the stack as a bool value.
- `LessThan` (`105`): Pops the two topmost values on from the stack, checks whether the second value is less than the first, then pushes the result onto the stack as a bool value.
- `Not` (`106`): Pops the topmost value from the stack, performs a logical not on it, then pushes the result onto the stack.
- `And` (`107`): Pops the two topmost values from the stack, performs a logical and on them, then pushes the result onto the stack.
- `Or` (`108`): Pops the two topmost values from the stack, performs a logical or on them, then pushes the result onto the stack.
- `GreaterThan` (`109`): Pops the two topmost values on from the stack, checks whether the second value is greater than the first, then pushes the result onto the stack as a bool value.
