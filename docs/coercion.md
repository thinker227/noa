# Coercion

This is a handy little table over how values behave when coerced to another type, eg. things like `!2` or `true > ()`. Vertical is the type of the value being coerced and horizontal is the type being coerced to. Coercion is implemented in [coercion.rs](src/runtime/src/runtime/value/coercion.rs).

Blank spaces indicate that the coercion is invalid.

|          | Number                        | Bool       | Function   | String                                        |
|----------|-------------------------------|------------|------------|-----------------------------------------------|
| Number   | same value                    | `true`     |            | formatted using `.` for the decimal separator |
| Bool     | `true` => `1`, `false` => `0` | same value |            | `true` => `"true"`, `false` => `"false"`      |
| Function |                               | `true`     | same value | name of the function                          |
| String   |                               | `true`     |            | same value                                    |
| Nil      | `0`                           | `false`    |            | `"()"`                                        |

Unlike languages like Javascript, numbers are *always* truthy, and *only* nil is falsey. This makes checking for nil simple and consistent in terms of behavior (`if x`). It is worth noting that the opposite is not true for coercing bools into numbers, so coercing a number into a bool and then back into a number will not return the same value as what it started with.

There is technically a nil type, but is impossible to coerce anything to it and every coercion to it fails, even nil to itself.

## Exit codes

When a program exits, the return value from the main function will be attempted to be coerced into a number using a set of special rules. This coercion will always succeed to prevent the program from crashing at the very last moment.

| Value    | Exit code                     |
|----------|-------------------------------|
| Number   | same value                    |
| Bool     | `true` => `0`, `false` => `1` |
| Function | `0`                           |
| String   | `0`                           |
| Nil      | `0`                           |

Notably, the coercion from bool is flipped compared to normal coercion. Returning a bool can be read as "did the program succeed?".

## Equality comparisons

Equality comparisons using `==` always compare values plainly, they do not perform any coercion, so two values of different types will *never* be equal.
