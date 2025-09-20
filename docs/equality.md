# Equality

Noa provides two [operators](./operators.md) for checking whether two things are equal to each other, `==` and `!=`. This document specifies the rules which these operators follow for determining whether two values are equal to each other.

## Numbers

Numbers in Noa are 64-bit IEEE floating-point numbers, and follow the same rules as all other floats. This means that they are subject to floating-point imprecision, such as `0.1 + 0.2` not equaling `0.3`. This is not a fault of Noa, rather floating-point numbers in general. Floating-point numbers also specify positive infinity being equal to itself (and same for negative infinity), and NaN never being equal to itself.

## Booleans

Not sure whether booleans need an explaination for equality, but regardless, `true` is equal to `true` and `false` is equal to `false`.

## Functions

Functions are only ever equal if **a\)** they refer to the same function or lambda, and **b\)** they share the same closure (by reference). This means that a comparison such as `print == print` is always true, and that a lambda which captures a variable will be equal to itself, but never any other "instance" of said lambda.

## Strings

Strings are equal if their UTF-8 binary representation is the same. This unfortunately runs into issues where two strings may look the same but have different representations. For instance, the strings `ö` and `ö` aren't equal, because the first is the character `ö` (U+00F6), and the second is the characters `o` (U+006F) and `◌̈` (U+0308).

## Objects

Objects are equal if they share the same fields and each field shares the same value in both objects. Field names are compared using the same rules as strings, and values are compared using the same equality rules as otherwise.

## Lists

Lists are equal if they are the same size and each element in the first list is equal to the element at the same index in the second list.

## Nil

Nil (`()`) is always equal to iself.
