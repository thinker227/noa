# Operators

This file contains some tables over the different operators Noa supports.

All operators will attempt to coerce their operands to their expected input type(s). As such, an expression such as `() + 1` won't fail, because `()` will first be coerced into a number.

## Unary operators

A unary operator is an operator which takes in a single operand, such as `-1`.

Operator | Operand | Result                               |
---------|---------|--------------------------------------|
`+`      | Number  | Same value as the operand (identity) |
`-`      | Number  | Negative of operand                  |

## Binary operators

A binary operator is an operator which takes in two operands, a left and right operand.

Operator | Left   | Right  | Result                                                                 |
---------|--------|--------|------------------------------------------------------------------------|
`+`      | Number | Number | The sum of left and right                                              |
`-`      | Number | Number | The difference between left and right                                  |
`*`      | Number | Number | The product of left and right                                          |
`/`      | Number | Number | The quotient of left and right                                         |
`==`     | Any    | Any    | Whether left and right are equal (see [equality](./equality.md))       |
`!=`     | Any    | Any    | Whether left and right are *not* equal (see [equality](./equality.md)) |
`<`      | Number | Number | Whether left is less than right                                        |
`>`      | Number | Number | Whether left is greater than right                                     |
`<=`     | Number | Number | Whether left is less than or equal to right                            |
`>=`     | Number | Number | Whether left is greater than or equal to right                         |

## Operator precedence

Operator precedence specifies in what order multiple chained operators parse. For instance, `1 + 2 * 3` is equivalent to `1 + (2 * 3)` because `*` has a *higher* precedence than `+`. If two operators with the same precedence are chained, then the operators are evaluated from left to right (all operators in Noa are left-associative).

The following is a list of operators and their precedence in order from lowest to highest:

1. `a == b`, `a != b`
2. `a < b`, `a > b`, `a <= b`, `a >= b`
3. `a + b`, `a - b`
4. `a * b`, `a / b`
5. `+x`, `-x`
