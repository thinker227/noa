<!-- markdownlint-disable MD033 MD041 -->

# Built-in functions

This is a list of all functions which come built into Noa.

Parameters surrounded in `[`brackets`]` indicate that the parameter is optional.

## Console

| Function | Description | Parameters | Returns | Runtime ID |
|----------|-------------|------------|---------|------------|
| `print` | Prints a value to the standard output. | `what`: The value to print. Calls `toString` to format the value into a string before printing it.<br/>`[appendNewline]`: Whether to append a newline character to the end of the string. Defaults to `true`. | `()` | `0x0` |
| `getInput` | Reads user input from the standard input. | | A string read from the standard input. | `0x1` |

## Files

| Function | Description | Parameters | Returns | Runtime ID |
|----------|-------------|------------|---------|------------|
| `readFile` | Reads the contents of a file as a string. | `path`: The path to the file to read. | The contents of the read file, as a string. Alternatively, `()` if the file for some reason could not be read. | `0x2` |
| `writeFile` | Write a string to a file. | `path`: The path to the file to write to.<br/>`content`: The content to write to the file, as a string. | `true` if the file was successfully written to, otherwise `false`. | `0x3` |

## Strings

| Function | Description | Parameters | Returns | Runtime ID |
|----------|-------------|------------|---------|------------|
| `toString` | Converts a value into a string representation. | `x`: The value to convert. | A string representation of the value. | `0x4` |

## Lists

| Function | Description | Parameters | Returns | Runtime ID |
|----------|-------------|------------|---------|------------|
| `push` | Pushes a value onto the end of a list, **mutating it in-place**. | `list`: The list to push to.<br/>`value`: The value to push. | `()` | `0x5` |
| `pop` | Pops a value from the end of a list. | `list`: The list to pop from. | The value popped from the list, or `()` if the list was empty. | `0x6` |
| `append` | Appends a value to the end of a list, **returning a new list** containing the original list with the value appended to the end. | `source`: The source list to append to.<br/>`value`: The value to append. | A new list containing the source list with the value appended to the end. | `0x7` |
| `concat` | Concatenates two lists together, **returning a new list**. | `source`: The source list.<br/`values`: The list to append to the end of the source list. | A new list containing the second list concatenates to the end of the source list. | `0x8` |
| `slice` | Creates a slice out of a list containing values of the list from a start index to an end index. | `source`: The source list to slice from.<br/>`start`: The *inclusive* start index to begin the slice from.<br/>`end`: The *exclusive* end index to end the slice at. | A new list containing the values of the source list from the start index to the end index. Returns an empty list if the start index is greater than the end index, or if the start index is outside the source list. | `0x9` |
| `map` | Maps the values of a list to a new set of values using a transform function, **returning a new list**. | `source`: The source list to transform.<br/>`transform`: A function to apply to each element of the list, where the value returned from the function will be the value at the same index in the new list. | A new list with the same length as the source list with every element transformed using the transform function. | `0xA` |
| `flatMap` | Maps the values of a list to a set of lists using a transform function, then flattens the lists into a list of values. | `source`: The source list to transform.<br/>`transform`: A function to apply to each element of the list, where the value returned from the function will be flattened and concatenated into the result list. The function must return a list. | A new list which consists of the values of the source list transformed using the transform function and then concatenates together into a resulting list. | `0xB` |
| `filter` | Filters the values of a list based on a predicate function, **returning a new list**. | `source`: The source list to filter.<br/>`predicate`: A function to apply to every element of the list where the return value specifies whether to include the value in the resulting list or not. Can return anything, but the value will be coerced into a boolean. | A new list consisting of the values of the source list filtered using the predicate function. | `0xC` |
| `reduce` | Performs a reduction (aka. "fold right" or "aggregate") operation on a list.<br/>Begins by taking a seed value and applying a function onto it and the first element of the list. The function is then applied again onto the resulting value and the second value of the list, and so forth for every element of the list. If the list is empty, the seed value is returned.<br/>For instance, `reduce([1, 2, 3], (r, x) => r + x)` returns the sum of all the elements in the list, `6`. | `source`: The source list.<br/>`function`: The function which reduces two values together into a single value. Should take two parameters where the first value is the current collected value and the second is the current element of the list, and should return a new collected value. | The value collected from repeatedly applying the function onto the collected value and each value of the list. | `0xD` |
| `reverse` | Reverses a list, **returning a new list**. | `source`: The source list to reverse. | The source list with the order of all elements reversed. | `0xE` |
| `any` | Checks whether any value of a list matches a predicate. Starts from the beginning of the list and checks the elements until an element either matches the predicate or the end of the list is reached. | `source`: The source list to check the elements of.<br/>`predicate`: A predicate function which will be applied to each element of the list. | `true` if any element of the list matches the predicate function, otherwise `false`. Returns `()` if the list is empty. | `0xF` |
| `all` | Checks whether all elements of a list match a predicate. Starts from the beginning of the list and checks the elements until an element either doesn't match or the end of the list is reached. | `source`: The source list to check the elements of.<br/>`predicate`: A predicate function which will be applied to each element of the list. | `true` if all elements of the list match the predicate function, otherwise `false`. Returns `()` if the list is empty. | `0x10` |
| `find` | Tries to find an element which matches a predicate within a list. | `source`: The source list to find the element within.<br/>`predicate`: A predicate function to apply to each element to check whether to return it.<br/>`fromEnd`: If `true`, the function will search from the end of the towards the start instead of from the start towards the end. | The first element within the list which matches the predicate. | `0x11` |
