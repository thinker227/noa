<!-- markdownlint-disable MD033 MD041 -->

<div align="center">
<img src="./noa.svg" alt="Noa logo" width="160", height="220">
<h1>Noa</h1>
</div>

> [!NOTE]
> Noa is currently a heavily work-in-progress project. The current state of the project does not fully represent what the final result is meant to look like.

**Noa** is a *dynamically typed*, *imperative*, *compiled* programming language. The language features a familiar C/JS/Rust-like syntax with static variable resolution (so no "variable is not defined" errors!) and a lightweight featureset. Jump down to the [samples](#samples) section for some code samples!

Noa compiles to its own cross-platform bytecode format *Ark* which in turn can be run through the Noa runtime.

In addition to the language itself, Noa also features a **VSCode extension** with full(-ish) language support! The extension currently supports *basic syntax highlighting*, *error messages*, *basic intellisense*, *go to definition*, *find all references*, and *renaming symbols!*

This merely a passion project of mine and is not meant to be taken seriously! It's not meant to be a "production-ready" language, but I hope to one day be able to write some somewhat useful programs in it.

## Installation

> [!NOTE]
> Noa can currently only be compiled from source.

<details>

<summary>Compile from source</summary>

To compile and install Noa from source, you need the [.NET 9 SDK and runtime](https://dotnet.microsoft.com/) and [Cargo](https://www.rust-lang.org/tools/install) to compile the compiler and runtime respectively. Once you have .NET and Cargo installed, follow these instructions:

1. Clone the repo using `git clone https://github.com/thinker227/noa.git`.
2. `cd` into the root of the project (the folder which contains this readme file).
3. Run the `update-tool.sh` script (or the commands therein, they're all just .NET commands) which will compile and install the complier as a .NET tool. Worry not, you can easily uninstall it using `dotnet tool uninstall noa --global`.
4. `cd` into `src/runtime` and run `cargo build -r` which will compile the runtime.
5. Locate the produced executable (which should be in `target/release` named `noa_runtime` or `noa_runtime.exe` on Windows).
6. Create an environment variable named `NOA_RUNTIME` containing the file path to the runtime executable. Alternatively you can specify the `--runtime <path>` command-line option when running `noa run` to manually specify the path to the runtime executable, however it's much simpler to use an environment variable.
7. You'll usually have to restart your terminal and/or pc for the environment variable and .NET tool to be available.

</details>

After everything has been installed, you can invoke the Noa CLI using the `noa` command from your terminal!

### VSCode extension

<details>

<summary>Compile from source</summary>

To compile and install the VSCode extension from source, you need [Node.js](https://nodejs.org) and [vsce](https://code.visualstudio.com/api/working-with-extensions/publishing-extension#vsce). Also make sure you have `code` available from the command line.

1. `cd` into `src/vscode-extension` and run `npm install` followed by `npm run compile`.
2. Run `vsce package --skip-license`.
3. Run `code --install-extension <path>`, replacing `<path>` with the file path to the `.vsix` file which `vsce` generated.

</details>

## Project status

- [x] AST
- [x] Lexer
- [x] Parser
- [x] Scope/symbol resolution
- [x] Flow analysis
- [ ] Optimization
- [x] Bytecode
- [x] Runtime
- [x] CLI
- [x] Language server

## Samples

> [!WARNING]
> Some of these samples may currently not work.

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
