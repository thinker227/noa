# Language

This document contains an overview of the Noa language.

Noa is a *dynamically typed*, *imperative*, *expression-oriented* programming language. This means that in Noa, you never have to declare the type of variables or functions, much like languages like Javascript or Python. Unlike Javascript and Python, however, Noa is *expression-oriented*, which means that almost everything in Noa is an expression which yields some value. Noa also has functions, much like you'd expect of any other modern language.

Noa is heavily influenced by Javascript and Rust, with its dynamic typing being taken from Javascript (though with much more sane rules), and its expression-orientedness being taken from Rust. For the most part, a lot of basic Javascript code is going *to be* valid Noa code, with perhaps some syntactic modifications.

A Noa program consists of a single file with the `.noa` extension. Within this file sits all the code which is going to be executed when the program runs, all wrapped in a [function](#functions).

## Functions

The single most basic building block of a Noa program is the *function*. A function, like in most other programming languages, is a container for code, which might take some amount of parameters and return some value. A function in Noa is declared using the `func` keyword, followed by a name and a list of parameters:

```js
func add(a, b) {
    return a + b;
}
```

This function `add` will take in two numbers, `a` and `b`, and return their sum. Since Noa is dynamically typed, you don't have to declare the types of `a` and `b`, nor the return type of the function.

Functions in Noa have two unique forms. The first is what's written above, where the function is followed by a block containing a series of statements. The second is somewhat different and uses the `=>` keyword.

```js
func add(a, b) => a + b;
```

These are two ways of writing the same function, they are functionally identical except for syntax. The `=>` here is followed by the expression returned by the function, which is equivalent to just writing a `return` statement in the function body.

Function parameters, just like [variables](#variable-declarations-and-assignment), are *immutable* by default, but can be declared as mutable using the `mut` keyword.

```rs
func f(mut x) {
    x = 1;
    print(x); // 1
}

f(0);
```

Functions are invoked similarly to other languages by writing out the name of the function, followed by parentheses containing the arguments to the function.

```js
add(1, 2)
```

The value yielded by this expression is the return value of the function.

However, what if the function doesn't return anything? In this case, the function will return `()`.

```js
func f() {
    
}
```

This function doesn't do anything, not even return a value, so evaluating it will return `()`.

Similarly, if you try to call a function but don't provide enough arguments, the missing arguments will be filled in by `()`.

```js
add(1);
// equivalent to
add(1, ());
```

Providing *too many* arguments to a function will simply discard the superfluous arguments.

```js
add(1, 2, 3);
// equivalent to
add(1, 2);
```

### Passing around functions

Functions in Noa are *first-class citizens*, just like in Javascript and Python. This means that functions are values, just like numbers, booleans, and everything else, and can be passed to other functions and returned from them. The quintessential example here is the `map` function:

```js
func add1(x) => x + 1;

let xs = map([1, 2, 3], add1);
print(xs); // [2, 3, 4]
```

On the other end is the `discard` function, which takes a value and returns a function:

```js
let f = discard(42);
let x = f(true);
print(x); // 42
```

Noa also has lambda expressions, [described in their own section](#lambdas).

## Statements

A statement, broadly speaking, is a piece of code which "does something", as opposed to an expression which just yields a value. Most things in Noa are expressions, but some select things are statements, namely [function declarations](#function-declarations), [variable declarations](#variable-declarations-and-assignment), and [assignment statements](#variable-declarations-and-assignment).

### Function declarations

A [function](#functions) declaration is a statement. This means that functions may be declared anywhere a statement is expected, including inside [block expressions](#blocks) and other functions.

### Variable declarations and assignment

A variable in Noa is declared using the `let` keyword, followed by the name of the variable, an equals sign (`=`), and then the expression to assign to the variable.

```js
let x = 0;
```

Variables *have* to have a value assigned to them on declaration, so declaring a variable using just `let x;` is invalid. If a variable should be "undeclared" and later assigned depending on a code path, it is recommended to initially assign it `()`.

Variables in Noa are *immutable* by default, meaning that their value cannot change after being declared.

```js
let x = 0;
x = 1; // error: Cannot assign to 'x' because it is immutable.
```

The `mut` keyword can be used to make a variable mutable, i.e. given the ability to change its value after declaration.

```rs
let mut x = 0;
x = 1;
```

Variables within the same scope may shadow each other, however variables may not shadow functions declared in the same scope.

```js
let x = 0;
let x = 1;
print(x); // 1

func f() {}
let f = true; // Variable 'f' shadows function 'f'.
```

Noa also supports *compound* assignment statements, i.e. assignments combined with an operator like `+` or `-`. The supported compund assignments are:

- `+=`
- `-=`
- `*=`
- `/=`

```rs
let mut x = 1;
x += 1;
x *= 2;

print(x); // 4
```

## Expressions

An expression in Noa is something which yields a *value*. Most things in Noa are expressions (which some exceptions), notably things like [numbers](#numbers), [booleans](#booleans), [strings](#strings), [`()`](#nil), [function calls](#functions), and [lambda expressions](#lambdas), but also some more peculiar things, such as [blocks](#blocks), [`return`](#return), [`break`](#break), and [`continue`](#continue).

### Numbers

Numeric literals describe decimal numbers. They consists of the digits 0 through 9, and can optionally contain a fractional component.

```js
1
42
0.3
0.621
```

Note that Noa does **not** support shorthand for decimal numbers where the whole part is 0, unlike some other languages which support `.5` for instance.

### Booleans

A boolean is either the value `true` or `false`.

### Strings

A string is a sequence of characters enclosed by double-quotes (`"`). The string may be any valid UTF-8 character sequence.

```js
""
"uwu"
"strange is the night"
"räksmörgås på skärgårdsö"
"this string is straight up bussin 🔥🔥🗣️"
"翻訳が下手な日本語"
```

String literals support some select escape sequences:

- `\n`: newline
- `\r`: carrage return
- `\t`: tab
- `\0`: null
- `\"`: `"`
- `\\`: `\`

```js
"\tline 1\r\n\t\"line 2\""
```

String literals also support interpolation, which allows you to embed expression within strings. String interpolation is written by surrounding an expression with curly-braces (`{}`) within a string literal. Within normal string literals, you have to write a `\` before the opening curly brace of an interpolation. However, you can also write a `\` before the opening double-quote of the string literal in order to make all curly-brace pairs within the literal into interpolations. Within literals beginning with `\"` you can still escape interpolations by writing yet another `\` before the opening `{`.

```js
"abc \{1} def" // abc 1 def
\"abc {2} def" // abc 2 def
\"abc \{3} def" // abc {3} def
```

### Nil

`()`, pronounced "nil" or "unit", is Noa's equivalent to `null` in most other languages. It is a *unit type*, which means there is only one possible value of it. It is generally a useless value because it doesn't support any operations and doesn't represent anything special, but it can be useful to signify failure. In most cases, attempting to do anything at all with `()` (for instance calling or performing arithmetic on it) will result in a runtime exception.

### Objects

Objects take their shape mainly from objects/dictionaries/tables in other dynamic languages Javascript or Lua. Objects in Noa, however, come in two flavors: *static* and *dynamic* objects.

*Static* objects (or just *objects* since these are the standard kind) act much like objects in other dynamic languages and are akin to dictionaries/maps - they bind values to names (keys). They are declared using curly braces (`{}`) containing a list of *fields*, each of which consists of a name followed by `:` and then the value bound to the name.

```js
// A simple object representing a person
let person = {
    name: "Voz",
    age: 29
};
```

To retrieve a value from an object, you use the familiar `.` operator followed by the name of the key.

```js
print(person.name); // Voz
```

Static objects have a couple differences from most other languages, however, in line with Noa's adoption of immutability; fields are *immutable* by default, meaning that they cannot be reassigned.

```js
let obj = { x: 1 };
obj.x = 2; // error: cannot write to immutable field "x"
```

If you want a field to be mutable, just write `mut` before the name.

```js
let obj = { mut x: 1 };
obj.x = 2; // fine
```

So what happens if you try to access a field which doesn't exist?

```js
let obj = {}; // empty object
print(obj.x); // error: field "x" does not exist
```

Well, you get a runtime error. But what happens if you try to *assign* to a field which doesn't exist?

```js
let obj = {}; // empty object
obj.x = "uwu"; // error: field "x" does not exist
```

You still get an error. This is contrary to what most other dynamic languages let you do with these kinds of objects. In Noa, static objects can only ever contain the fields you declare them with, you cannot add new fields after the fact.

[*Or can you?*](https://youtube.com/watch?v=TN25ghkfgQA) This is where *dynamic* objects come in useful. If you want an object which can dynamically grow in its amount of fields, you can use dynamic objects.

Dynamic objects are declared similarly to static objects, except for the addition of the `dyn` keyword before the opening curly brace.

```js
let dynObj = dyn {};

dynObj.a = 1;
dynObj.b = "owo";
dynObj.c = false;

print(dynObj); // { "a": 1, "b": "owo", "c": false }
```

Fields in dynamic objects are additionally always mutable, so the `mut` keyword cannot be used.

It is also worth noting that fields in objects will be always iterated and printed in the order that they are declared/added; they are *well-ordered*.

#### Syntax sugar

Finally, declaring and accessing fields in objects have a couple handy shorthands and sets of syntax sugar.

When declaring the name of or accessing a field, you can use either a simple name (like `x`), a string (like `"a string"`), or an expression surrounded by parentheses (like `(true)`). The keys in objects are actually just strings, so any name you give to a field is just converted into a string, including expressions.

```js
let obj = {
    x: 1,
    "a string": "another string",
    (true): false
};

print(obj.x); // 1
print(obj."a string"); // "another string"
print(obj.(true)); // false

print(obj."x"); // 1
print(obj."true"); // false
```

The string literals used for field names also support string interpolation.

```js
let str = "a";
let obj = {
    \"{str} b": "c"
};

print(obj); // { "a b": "c" }

print(obj.\"{str} b"); // "c"
```

If you want to assign a field in an object to the value of a variable and also name the field the same as the variable, you can simply omit the name of the field.

```js
let a = 1;
let b = "owo";
let obj = {
    :a,
    :b
};

print(obj); // { "a": 1, "b": "owo" }
```

This also works with a handful of other expressions; variables (`:x`), access expressions (`:a.b`), nested access expressions (`:a.b.c`), and block expressions if the trailing expression also shares this property (`:{ let x = 0; x }`).

### Lists

Lists in Noa are dynamic sequences of values. Just like arrays/lists/vectors in other languages, they can be constructed with a set of elements and indexed by element.

Lists are declared using square brackets (`[]`) containing a comma-separated sequence of values.

```js
let xs = ["a", "b", "c"];
```

Indexing lists is done by putting square brackets after an expression with the index between the brackets.

```js
print(xs[1]); // "b"
```

Lists are dynamic and mutable, meaning that they can have elements added/remove to/from them, and elements can be mutated whenever.

```js
xs[2] = "d";

print(xs[2]); // "d"
```

However, elements cannot be added outside the bounds of a list. Unlike some languages, negative indices always cause an error.

```js
print(xs[3]); // error: index 3 is outside the bounds of the list
print(xs[-1]); // error: index -1 is outside the bounds of the list
```

When indexing lists, the index is rounded *towards 0*.

```js
print(xs[0.5]); // "a"
print(xs[1.9]); // "b"
print(xs[2.1]); // "d"
```

Indexing a list using NaN or positive or negative infinity causes en error.

### Blocks

Unlike a lot of other C-like languages, blocks in Noa are expressions. Blocks can contain zero or more statements, and optionally end with a trailing expression which will be yielded as the value of the expression.

```js
let x = {
    let v = 1 + 2;
    v
};

print(x); // 3
```

If a block doesn't end with a trailing expressions, the value of the block will be `()`.

```js
let x = {
    print("uwu");
};

print(x); // ()
```

A block can also be used as a stand-alone statement. This is mainly useful to control variable scope.

```js
let x = 1;

{
    let a = 2;
    print(x); // 2
}

print(x); // 1
```

However, an *empty* block may not be used as a stand-alone statement.

Note that the block which makes up a [function](#functions) body is a normal block expression, so you may return a value from a function by simply writing the return value at the very end of the block:

```js
func f(x) {
    print(x);
    x
}

let v = f(2);
print(v); // 2
```

### Loops

`loop` is used to enter a normal unconditional loop. A loop is written using the `loop` keyword followed by a block for the body of the loop. The body will repeat forever until a `break` expression is encountered.

```js
// Prints the numbers 1 through 10.

let mut i = 1;

loop {
    print(i);
    
    i += 1;

    if (i > 10) {
        break;
    }
}
```

If a `break` expression with a value is encountered, the value the `loop` yields will be the value of the `break` expression.

```js
// Prints how many tries it took for roll a
// number greater than 9 between 0 and 10.

let mut i = 1;

let x = loop {
    if (random(0, 10) >= 9) {
        break i;
    }

    i += 1;
};

print(x);
```

### Return

`return` is used to return a value from a [function](#functions). `return` may also be used without a value, which will return `()`. `return` is an expression, which means that you can use it anywhere a normal expression would be expected. *In theory* `return` yields a value, however due to affecting the flow of the program by returning from the current function, the value yielded by `return` is considered *unreachable*. (If you really want to put a name to this, the value yielded by `return` is "never".)

```js
let x = return 1;
print(x); // this will never execute
```

### Break

`break` is used to break out of a loop, and optionally yield a value from the loop. If used without a value, the loop will yield `()`. Just like `return`, `break` is an expression, which means that you can use it anywhere a normal expression would be expected. The value it yields is also considered *unreachable*.

### Continue

`continue` is used to continue onto the need iteration of a loop. It will skip all following code within the loop and jump back to the start of it. Just like `return` and `break`, `continue` is an expression, which means that you can use it anywhere a normal expression would be expected. The value it yields is also considered *unreachable*.

### Lambdas

A lambda expression, sometimes called an *anonymous function*, is a function declared in-place as an expression. A lambda expression is written by listing its parameters within parentheses, followed by `=>`, and then an expression to return. To demonstrate, the following snippets of code are equivalent:

```js
let f = (x) => x + 1;
```

```js
func g(x) {
    return x + 1;
}
let f = g;
```

If you want to execute code within a block, just like a normal function, but within a lambda, simply write a block after the `=>`.

```js
let f = () => {
    print("owo");
};
```

Lambda parameters, just like [function parameters](#functions) and [variables](#variable-declarations-and-assignment), are *immutable* by default, but can be declared as mutable using the `mut` keyword.

```rs
let f = (mut x) => {
    x = 1;
    print(x); // 1
};

f(0);
```

#### Captures

Lambdas, unlike functions, have the special property that they may capture variables from their environment:

```js
let x = 1;

let printX = () => {
    print(x);
};

printX(); // 1
```

Variables captured by lambdas can still be mutated externally and the change will be observed inside the lambda as well.

```rs
let mut x = 1;

let printX = () => {
    print(x);
};

x = 42;

printX(); // 42
```

... and same for mutating a variable from inside a lambda.

```rs
let mut x = 4;

let addToX = (val) => {
    x = x + val;
};

addToX(8);
print(x); // 12
```
