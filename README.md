# Noa

*Simple* programming language.

## Goals

- Slim language
- JS/Rust-like syntax
- Dynamically typed
- Strongly typed
- Compiles to bytecode
- Runs on a VM
- Compiler implemented in C#
- Runtime implemented in Rust
- Language server

## Status 

- [x] AST
- [x] Lexer
- [ ] Parser
- [ ] Scope/symbol resolution
- [ ] Optimization
- [ ] Bytecode
- [ ] Runtime
- [ ] CLI
- [ ] Language server

## Samples

```js
print("Hello world!");
```

```js
let x = 0;

x = 1; // ERROR: x is immutable

let mut y = 2;

y = 3; // fine
```

```js
let x = {      // Block expression
    let a = 1;
    let b = 2;
    a + b      // Implicit return from block
};
```

```js
let add = (a, b) => a + b;

let num = add(1, 2);
```

```js
let greet = (name) => {
    print("Hello, " + name + "!");
};

let name = readLine();
greet(name);
```

### Dreamlands

```js
// Import module
import "module.noa" (foo, bar, baz);

// Export stuff
export (val, hello);

let val = 69;
let hello = () => print("Hello!!!");
```

```js
// Custom infix operator (extremely provisional syntax)
($) = (f, x) => f(x);

// Usage
let v = ((k) => k + 1) $ 2;
```
