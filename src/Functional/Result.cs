// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents a result of an operation that can either be a success or a failure, and carries a value of the result of type T
/// in case of success. Note that <typeparamref name="T"/> must be a non-nullable type, i.e. void must be represented by
/// <see cref="Unit"/> and <see cref="string"/>? is not allowed. Functional means no <see langword="null"/>-s.
/// </summary>
/// <typeparam name="T">
/// The type of the value carried by the Result in case of a successful operation.
/// </typeparam>
public readonly record struct Result<T> where T : notnull
{
    /// <summary>
    /// The value associated with a successful Result, otherwise it is intentionally <see langword="null"/> and it is not used.
    /// </summary>
    readonly T? _value;

    /// <summary>
    /// The error associated with a failed Result. If the Result represents a success, this field is intentionally
    /// <see langword="null"/> and it is not used. If the Result represents a failure and the error is not set, it will be
    /// replaced by <see cref="DefaultError.Instance"/> on the fly.
    /// </summary>
    readonly Error? _error;

    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    readonly bool _isSuccess;

    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Indicates whether the result represents a failed operation.
    /// is a success.
    /// </summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Initializes a new instance of the Result&lt;T&gt; struct representing a successful result with the specified value.
    /// This constructor is private and can only be called from within the class.
    /// </summary>
    /// <remarks>
    /// Note that the constructor of Result&lt;T&gt; is private and cannot be accessed directly from outside the class.
    /// For creating a successful Result, use this static <see cref="Ok"/> method.
    /// For creating a failed Result, use the static <see cref="Fail"/> method.
    /// </remarks>
    Result(T value)
    {
        _value = value;
        _isSuccess = true;
    }

    /// <summary>
    /// Initializes a new instance of the Result&lt;T&gt; struct representing a failed result with the specified error.
    /// This constructor is private and can only be called from within the class.
    /// </summary>
    /// <remarks>
    /// Note that the constructor of Result&lt;T&gt; is private and cannot be accessed directly from outside the class.
    /// For creating a successful Result, use the static <see cref="Ok"/> method.
    /// For creating a failed Result, use this static <see cref="Fail"/> method.
    /// </remarks>
    Result(Error error) => _error = error;

    /// <summary>
    /// Creates a new Result instance representing a successful operation that returns the specified value.
    /// </summary>
    /// <param name="value">
    /// The value of the result. This parameter must not be null.
    /// </param>
    public static Result<T> Ok(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(value);
    }

    /// <summary>
    /// Creates a new Result instance representing a failed operation with the specified error.
    /// </summary>
    /// <param name="e">
    /// The error that caused the result to be a failure. This parameter must not be null.
    /// </param>
    public static Result<T> Fail(Error e)
    {
        ArgumentNullException.ThrowIfNull(e);
        return new(e);
    }

    /// <summary>
    /// Gets the value associated with a successful Result. If the Result represents a failure, accessing this property will throw
    /// an InvalidOperationException.
    /// </summary>
    public T Value => _isSuccess ? _value! : throw new InvalidOperationException("Result is a failure; it has no Value.");

    /// <summary>
    /// Gets the error associated with a failed Result. If the Result represents a success, accessing this property will throw
    /// an InvalidOperationException.
    /// </summary>
    public Error Error => !_isSuccess ? _error ?? DefaultError.Instance : throw new InvalidOperationException("Result is a success; it has no Error.");

    /// <summary>
    /// Implicit converter from the type T value to Result&lt;T&gt;. This allows a value of type T to be automatically converted
    /// to a successful Result&lt;T&gt;.
    /// </summary>
    /// <param name="value">The value of the result. This parameter must not be null.</param>
    public static implicit operator Result<T>(T value) => Ok(value);

    /// <summary>
    /// Implicit converter from the Error type to Result&lt;T&gt;. This allows an Error to be automatically converted
    /// to a failed Result&lt;T&gt;.
    /// </summary>
    /// <param name="error"></param>
    public static implicit operator Result<T>(Error error) => Fail(error);
}
