# Ark bytecode format

This is a "specification" of the `.ark` file format which is used as Noa's IR format.

Ark file parsing is implemented in [ark.rs](/src/runtime/src/ark.rs).

## Structure

An ark file begins with a header which is followed by a series of sections.

- [Header](#header)
- [Function section](#function-section)
- [Code section](#code-section)
- [String section](#string-section)

## Header

The header contains the identifier for the Ark file (which is always the same), as well as metadata about the program.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 8 | `identifier` | The constant bytes `[116, 111, 116, 104, 101, 97, 114, 107]` (`totheark`). |
| 8 | 4 | `main` | Function ID of the main function, encoded as a [function ID](#function-id). |

## Function section

The function section specifies the functions of the program.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `functions_length` | The amount of following functions. |
| 4 | * | `functions` | A list of [functions](#function), of which the first one begins at the current byte offset. |

## Function

A function specifies an executable function.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `id` | The unique ID of the function, encoded as a [function ID](#function-id). |
| 4 | 4 | `name_index` | The string index of the name of the function. |
| 8 | 4 | `arity` | The amount of parameters to the function. |
| 12 | 4 | `locals_count` | The amount of locals allocated to the function. |
| 16 | 4 | `address` | The bytecode address within the code section where the function starts. |

## Function ID

A function ID is an unsigned 32-bit integer, where the most significant bit (bit 32) encodes whether the function is native or not.

### Example

```c
decimal:  621
decoded:  621
binary:   00000000 00000000 00000010 01101101
          ^
      native bit

decimal:  2147484574
decoded:  926
binary:   10000000 00000000 00000011 10011110
          ^
      native bit
```

## Code section

The code section contains all the bytecode which makes up the body of each function. The bytecode is stored back-to-back with a single 0xFF (boundary) opcode separating them.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `code_length` | The amount of following bytecode bytes. |
| 4 | * | `code` | A list of byte-encoded [opcodes](./opcodes.md). |

## String section

The string section contains all the constant strings of the program, like function names and string literals.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `strings_length` | The amount of following strings. |
| 4 | * | `strings` | A list of [strings](#string), of which the first one begins at the current byte offset. |

## String

A string is a UTF-8 encoded constant string.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 4 | `length` | The length of the string in bytes. |
| 4 | * | `bytes` | The bytes which make up the codepoints of the string. |
