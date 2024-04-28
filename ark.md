This is a "specification" of the `.ark` file format which is used as Noa's IR format.

## Structure

An ark file begins with a header which is followed by a series of sections.

- [Header](#header)
- [Function section](#function-section)

## Header

The header contains the identifier for the Ark file as well as metadata about the program.

| Byte offset | Bytes | Name | Description |
|-------------|-------|------|-------------|
| 0 | 8 | `identifier` | The constant bytes `[116, 111, 116, 104, 101, 97, 114, 107]` (`totheark`). |
| 0 | 4 | `main` | Function ID of the main function. |

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
| 4 | 4 | `code_length` | The length of the function in bytes. |
| 8 | * | `code` | The op-codes which make up the body of the function. |
