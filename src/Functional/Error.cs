// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents an error with a code and a message.
/// </summary>
/// <param name="Code"></param>
/// <param name="Message"></param>
public record Error(string Code, string Message)
{
    /// <summary>
    /// Static instance of the Error class representing no error.
    /// </summary>
    public static readonly Error None = new("", "");

    /// <summary>
    /// Creates a new Error instance representing a validation error with the specified error messages.
    /// </summary>
    /// <param name="errors">
    /// The collection of error messages that caused the validation error. This parameter must not be null.
    /// </param>
    /// <returns>
    /// An Error instance representing the validation error.
    /// </returns>
    public static Error Validation(IEnumerable<Error> errors) => new AggregateError(errors);
}

/// <summary>
/// Represents an aggregate error that contains multiple error messages.
/// </summary>
/// <param name="Errors"></param>
public sealed record AggregateError(IEnumerable<Error> Errors) : Error("aggregate", string.Join("\n", Errors.Select(e => e.Message)));
