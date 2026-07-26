// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

// A value-equal reference type (the common domain case).
sealed record Person(string Name, int Age);

// A reference-equal type (default identity equality) to prove Option delegates to the type's own
// equality via EqualityComparer<T>.Default rather than imposing value semantics of its own.
sealed class Box(int value)
{
    public int Value { get; } = value;
}

// A concrete Error for the fixtures — Error itself is abstract, and callers are expected to
// discriminate by type, not by string-sniffing Code.
sealed record TestError(string Code, string Message) : Error(Code, Message);

// We'll be using these aliases to make the monadic laws read like a textbook.
static class FunctionalAliases
{
    public static Option<T> Return<T>(T x) where T : notnull => Some(x);

    public static Result<T> Right<T>(T x) where T : notnull => Ok(x);

    public static Result<T> Left<T>(Error error) where T : notnull => Fail<T>(error);
}
