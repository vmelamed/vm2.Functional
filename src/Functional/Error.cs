// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents an error with a code and a message.
/// </summary>
public abstract record Error
{
    /// <summary>
    /// Initializes an <see cref="Error"/> instance with a code and a message.
    /// </summary>
    /// <param name="code">The error's code, e.g., common for a category of errors.</param>
    /// <param name="message">The error's individual message.</param>
    protected Error(string code, string message)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code    = code;
        Message = message;
    }

    /// <summary>
    /// Gets the instance's code that might be common for a category of <see cref="Error"/>s.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the instance's individual message that might be unique for this instance only.
    /// </summary>
    public string Message { get; }
}

/// <summary>
/// The error carried by an uninitialized default(Result&lt;T&gt;).
/// </summary>
public sealed record DefaultResultError : Error
{
    /// <summary>
    /// The <see cref="Error.Code"/> of every <see cref="DefaultResultError"/> instance.
    /// </summary>
    public const string ErrorCode = "result.uninitialized";

    DefaultResultError() : base(ErrorCode, "Unsuccessful Result<T> instance must be initialized with explicit error instance. It MUST NOT be default(Result<T>). Use DefaultResultError.Instance instead.") {}

    /// <summary>
    /// Gets the singleton instance of the <see cref="DefaultResultError"/> class.
    /// </summary>
    public static DefaultResultError Instance { get; } = new();
}

/// <summary>
/// Represents an aggregate error that contains multiple <see cref="Error"/> objects.
/// </summary>
public sealed record AggregateError : Error
{
    /// <summary>
    /// The <see cref="Error.Code"/> of every <see cref="AggregateError"/> instance.
    /// </summary>
    public const string ErrorCode = "error.aggregate";

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateError"/> class with the specified collection of errors. The error
    /// messages from the collection are concatenated into a single message for the base Error class.
    /// </summary>
    /// <param name="errors">
    /// The collection of <see cref="Error"/> objects that are aggregated into this aggregate error. This parameter must not be
    /// null, empty, or contain null elements.
    /// </param>
    public AggregateError(params IEnumerable<Error> errors) : base(ErrorCode, InitFromErrors(errors, out var immutableErrors))
        => Errors = immutableErrors;

    /// <summary>
    /// Gets the collection of <see cref="Error"/>s that are aggregated into this <see cref="AggregateError"/>.
    /// </summary>
    public IEnumerable<Error> Errors { get; }

    static string InitFromErrors(IEnumerable<Error> errors, out ImmutableList<Error> immutableErrors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        immutableErrors = [.. errors];

        if (immutableErrors.IsEmpty)
            throw new ArgumentException("The Error sequence must not be empty.", nameof(errors));
        if (immutableErrors.Contains(null!))
            throw new ArgumentException("The Error sequence must not contain null elements.", nameof(errors));

        return string.Join("\n", immutableErrors.Select(e => e.Message));
    }

    /// <inheritdoc />
    public bool Equals(AggregateError? other)
        => other is not null &&
           Enumerable.SequenceEqual(Errors, other.Errors);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hc = new();

        foreach (var e in Errors)
            hc.Add(e);

        return hc.ToHashCode();
    }
}
