# Noa

Noa is a work-in-progress dynamically typed, imperative, compiled programming language with its own bytecode format ("Ark") which runs on a virtual machine. Noa takes heavy influence from Rust and Javascript for its featureset, and C# for the runtime.

The compiler is implemented in C# and runtime in Rust.

## Goals

- Slim/minimal language
- JS/Rust-like syntax
- Dynamically typed
- Static variable resolution
- Compiles to bytecode
- Runs on a VM
- Compiler implemented in C#
- Runtime implemented in Rust
- Language server

## Status 

- [x] AST
- [x] Lexer
- [x] Parser
- [x] Scope/symbol resolution
- [x] Flow analysis
- [ ] Optimization
- [ ] Bytecode
- [ ] Runtime
- [x] CLI
- [x] Language server

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
func add(a, b) => a + b;

let num = add(1, 2);
```

```js
func greet(name) {
    print("Hello, " + name + "!");
};

let name = readLine();
greet(name);
```

```js
func createCounter() {
  let mut x = 0;
  () => {
    x += 1;
    x
  }
}

let counter = createCounter();
print(counter()); // 1
print(counter()); // 2
print(counter()); // 3
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
