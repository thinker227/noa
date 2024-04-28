This is a handy little table over how values behave when coerced to another type, eg. things like `!2` or `true > nil`. Vertical is the type of the value being coerced and horizontal is the type being coerced to. Coercion is implemented in [coercion.rs](src/runtime/src/runtime/value/coercion.rs).

Blank spaces indicate that the coercion is invalid.

|          | Number                        | Bool       | Function
|----------|-------------------------------|------------|----------
| Number   | same value                    | `true`     |
| Bool     | `true` => `1`, `false` => `0` | same value | 
| Function |                               | `true`     | same value
| Nil      | `0`                           | `false`    |

Unlike languages like Javascript, numbers are *always* truthy, and *only* nil is falsey. This makes checking for nil simple and consistent in terms of behavior (`if x`). It is worth noting that the opposite is not true for coercing bools into numbers, so coercing a number into a bool and then back into a number will not return the same value as what it started with.

There is technically a nil type, but is impossible to coerce anything to it and every coercion to it fails, even nil to itself.
