This is a "specification" of the `.ark` file format which is used as Noa's IR format.

Ark file parsing is implemented in [ark.rs](src/runtime/src/ark.rs), [function.rs](src/runtime/src/runtime/function.rs), and [opcode.rs](src/runtime/src/runtime/opcode.rs).

## Structure

An ark file begins with a header which is followed by a series of sections.

- [Header](#header)
- [Function section](#function-section)
- [String section](#string-section)

## Header

The header contains the identifier for the Ark file (which is always the same), as well as metadata about the program.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 8 | `identifier` | The constant bytes `[116, 111, 116, 104, 101, 97, 114, 107]` (`totheark`). |
| 8 | 4 | `main` | Function ID of the main function. |

## Function section

The function section specifies the functions of the program.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `functions_length` | The length of the following functions in bytes. |
| 4 | * | `functions` | A list of [functions](#function), of which the first one begins at the current byte offset. |

## Function

A function specifies an executable function.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `id` | The unique ID of the function. |
| 4 | 4 | `name_index` | The string index of the name of the function. |
| 8 | 4 | `code_length` | The length of the following code in bytes. |
| 12 | * | `code` | The op-codes which make up the body of the function. |

## String section

The string section contains all the constant strings of the program, like function names and string literals.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `strings_length` | The length of the following strings in bytes. |
| 4 | * | `strings` | A list of [strings](#string), of which the first one begins at the current byte offset. |

## String

A string is a UTF-8 encoded constant string.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `length` | The length of the string in bytes. |
| 4 | * | `bytes` | The bytes which make up the codepoints of the string. |
