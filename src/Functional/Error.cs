// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents an error with a code and a message.
/// </summary>
/// <param name="Code">A string representing the error code.</param>
/// <param name="Message">Detailed message describing the error.</param>
public abstract record Error(string Code, string Message);

/// <summary>
/// Represents the error to not initialize a type if its default state is forbidden, e.g. for <see cref="Result{T}"/> -- <c>default(Result&lt;T&gt;)</c>.
/// </summary>
public sealed record DefaultResultError() : Error("default", "default(Result<T>) is not allowed.")
{
    /// <summary>
    /// Gets the singleton instance of the DefaultError class.
    /// </summary>
    public static DefaultResultError Instance { get; } = new();
}

/// <summary>
/// Represents an aggregate error that contains multiple error messages.
/// </summary>
public sealed record AggregateError : Error
{
    /// <summary>
    /// Initializes a new instance of the <c>AggregateError</c> class with the specified collection of errors. The error
    /// messages from the collection are concatenated into a single message for the base Error class.
    /// </summary>
    /// <param name="errors">
    /// The collection of <see cref="Error"/> objects that are aggregated into this aggregate error. This parameter must not be
    /// null or empty.
    /// </param>
    public AggregateError(params IEnumerable<Error> errors) : base("aggregate", InitFromErrors(errors, out var immutableErrors))
        => Errors = immutableErrors;

    /// <summary>
    /// Gets the collection of error messages that are aggregated into this aggregate error.
    /// </summary>
    public IEnumerable<Error> Errors { get; init; }

    static string InitFromErrors(IEnumerable<Error> errors, out ImmutableList<Error> immutableErrors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        immutableErrors = errors.ToImmutableList();
        if (!immutableErrors.Any())
            throw new ArgumentException("The input collection of errors must not be empty.", nameof(errors));

        return string.Join("\n", immutableErrors.Select(e => e.Message));
    }
}
