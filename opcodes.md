- `NoOp` (`0`): No operation.

## Control flow (`1`-`13`)
- `Jump <address>` (`1`): Jumps to `address` within the current function.
- `JumpIf <address>` (`2`): Pops the topmost value from the stack and jumps to `address` if the value is `true`.
- `Call <arg count>` (`3`): Begins by looking up the element at `arg count` from the top of the stack and coerces it into a function. The remaining `arg count` elements are used as arguments, and the function is called.
- `Ret` (`4`): Pops the topmost value from the stack, returns from the current function, then pushes the value onto the stack.

## Stack push operations (`14`-`31`)
- `PushInt <int32>` (`14`): Pushes the 32-bit signed integer `int32` onto the stack.
- `PushBool <bool>` (`15`): Pushes the boolean `bool` onto the stack.
- `PushFunc <func id>` (`16`): Pushes the function with the ID `func id` onto the stack.
- `PushNil` (`17`): Pushes nil onto the stack.

## Miscellaneous stack operations (`32`-`45`)
- `Pop` (`32`): Pops the topmost value from the stack.
- `Dup` (`33`): Duplicates the topmost value on the stack and pushes it to the stack.
- `Swap` (`34`): Pops the two topmost values from the stack and pushes them back in reverse order.

## Locals operations (`46`-`63`)
- `StoreVar <var index>` (`46`): Pops the topmost value from the stack and stores it into a local variable with the index `var index`.
- `LoadVar <var index>` (`47`): Loads the value of a local variable with the index `var index` and pushes it onto the stack.

## Value operations (`64`-`EF`)
- `Add` (`64`): Pops the two topmost values from the stack, adds them together, then pushes the result onto the stack.
- `Sub` (`65`): Pops the two topmost values from the stack, subtracts the first value from the second, then pushes the result onto the stack.
- `Mult` (`66`): Pops the two topmost values from the stack, multiplies them together, then pushes the result onto the stack.
- `Div` (`67`): Pops the two topmost values from the stack, divides the second value by the first, then pushes the result onto the stack.
- `Equal` (`68`): Pops the two topmost values from the stack, compares them for equality, then pushes the result onto the stack as a bool value.
- `LessThan` (`69`): Pops the two topmost values on from the stack, checks whether the second value is less than the first, then pushes the result onto the stack as a bool value.
- `Not` (`6A`): Pops the topmost value from the stack, performs a logical not on it, then pushes the result onto the stack.
- `And` (`6B`): Pops the two topmost values from the stack, performs a logical and on them, then pushes the result onto the stack.
- `Or` (`6C`): Pops the two topmost values from the stack, performs a logical or on them, then pushes the result onto the stack.
- `GreaterThan` (`6D`): Pops the two topmost values on from the stack, checks whether the second value is greater than the first, then pushes the result onto the stack as a bool value.

## Operational instructions (`F0`-`FF`)
- `Boundary` (`FF`): Specifies a boundary between two functions. Produces a runtime exception.
