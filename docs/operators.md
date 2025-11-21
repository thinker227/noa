# Operators

This file contains some tables over the different operators Noa supports.

All operators will attempt to coerce their operands to their expected input type(s). As such, an expression such as `() + 1` won't fail, because `()` will first be coerced into a number.

## Unary operators

A unary operator is an operator which takes in a single operand, such as `-1`.

Operator | Operand | Result                               |
---------|---------|--------------------------------------|
`+`      | Number  | Same value as the operand (identity) |
`-`      | Number  | Negative of operand                  |
`not`    | Boolean | Logical negation of operand          |

## Binary operators

A binary operator is an operator which takes in two operands, a left and right operand.

Operator | Left    | Right   | Result                                                                 |
---------|---------|---------|------------------------------------------------------------------------|
`+`      | Number  | Number  | The sum of left and right                                              |
`-`      | Number  | Number  | The difference between left and right                                  |
`*`      | Number  | Number  | The product of left and right                                          |
`/`      | Number  | Number  | The quotient of left and right                                         |
`==`     | Any     | Any     | Whether left and right are equal (see [equality](./equality.md))       |
`!=`     | Any     | Any     | Whether left and right are *not* equal (see [equality](./equality.md)) |
`<`      | Number  | Number  | Whether left is less than right                                        |
`>`      | Number  | Number  | Whether left is greater than right                                     |
`<=`     | Number  | Number  | Whether left is less than or equal to right                            |
`>=`     | Number  | Number  | Whether left is greater than or equal to right                         |
`or`     | Boolean | Boolean | The logical disjunction of left and right                              |
`and`    | Boolean | Boolean | The logical conjunction of left and right                              |
`.`      | Object  | Field   | The value of the field in the object left with the field right         |

### Short-circuiting

Some operators, namely `or` and `and`, only evaluate their right-side operand if necessary.

- For `or`, the right-side operand is only evaluated if the left-side operand is `false`.
- For `and`, the right-side operand is only evaluated if the left-side operand is `true`.

## Operator precedence

Operator precedence specifies in what order multiple chained operators parse. For instance, `1 + 2 * 3` is equivalent to `1 + (2 * 3)` because `*` has a *higher* precedence than `+`. If two operators with the same precedence are chained, then the operators are evaluated from left to right (all operators in Noa are left-associative). For instance, member access and function invocation have the same precedence and can therefore be chained like `obj.f()`.

Notably, `not x` has the lowest precedence out of any operator. This is different from most other languages where `!` has the same precedence as the other unary operators `+x` and `-x`. This is because Noa uses keywords for these operators instead of symbols, so that `not a or b` works consistently with the way most people will mentally parse the phrase "not a or b" as "not a or not b".

The following is a list of operators and their precedence in order from lowest to highest:

1. `not x`
2. `a or b`
3. `a and b`
4. `a == b`, `a != b`
5. `a < b`, `a > b`, `a <= b`, `a >= b`
6. `a + b`, `a - b`
7. `a * b`, `a / b`
8. `+x`, `-x`
9. `obj.x`, `list[i]`, `f(x)`
