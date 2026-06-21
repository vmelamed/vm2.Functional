// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents a result of an operation that can either be a success or a failure. Key structure in functional programming for
/// handling operations that may fail without using exceptions. A.k.a. Railway Oriented Programming (ROP).
/// </summary>
public readonly record struct Result
{
    readonly Error? _error;

    /// <summary>
    /// Indicates whether the result represents a successful operation. It is true if the result is a success, and false if it
    /// is a failure.
    /// </summary>
    public bool IsSuccess => _error is null;

    /// <summary>
    /// Indicates whether the result represents a failed operation. It is true if the result is a failure, and false if it
    /// is a success.
    /// </summary>
    public bool IsFailure => _error is not null;

    /// <summary>
    /// Initializes a new instance of the Result struct representing a failure with the specified error.
    /// </summary>
    /// <param name="error">
    /// The error that caused the result to be a failure. This parameter must not be null.
    /// </param>
    /// <remarks>
    /// This constructor is private and is used internally to create a Result representing a failure.
    /// For creating a successful Result, use the static <see cref="Ok"/> method.
    /// For creating a failed Result, use the static <see cref="Fail"/> method.
    /// </remarks>
    Result(Error error) => _error = error;

    /// <summary>
    /// Creates a new Result instance representing a successful operation. This is the default state of a Result.
    /// </summary>
    public static Result Ok() => default;             // success *is* the default; nothing to set

    /// <summary>
    /// Creates a new Result instance representing a failed operation with the specified error.
    /// </summary>
    /// <param name="error">
    /// The error that caused the result to be a failure. This parameter must not be null.
    /// </param>
    public static Result Fail([NotNull] Error error)
    {
        ArgumentNullException.ThrowIfNull(error);     // null error would masquerade as success
        return new(error);
    }

    /// <summary>
    /// Gets the error associated with a failed Result. If the Result represents a success, accessing this property will throw
    /// an InvalidOperationException.
    /// </summary>
    public Error Error => _error ?? throw new InvalidOperationException("Result is a success; it has no Error.");
}

/// <summary>
/// Represents a result of an operation that can either be a success or a failure, and carries a value of the result of type T
/// in case of success.
/// </summary>
/// <typeparam name="T">
/// The type of the value carried by the Result in case of a successful operation.
/// This type parameter defines the type of the Value property when the Result is a success.
/// It can be any type, including reference types and value types.
/// If the Result represents a failure, the Value property will not be accessible and attempting to access it will throw an InvalidOperationException.
/// The Error property will contain the error information in case of a failure.
/// This allows for strong typing of the result value while still providing a mechanism to represent failure without exceptions.
/// </typeparam>
public readonly record struct Result<T>
{
    /// <summary>
    /// The value associated with a successful Result. It is null if the result is a failure.
    /// </summary>
    readonly T? _value;

    /// <summary>
    /// The error associated with a failed Result. It is null if the result is a success.
    /// </summary>
    readonly Error? _error;

    /// <summary>
    /// Indicates whether the Result instance has been initialized. This is used to track whether the Result has been assigned a
    /// value or an error and guards against <c>default(Result&lt;T&gt;)</c> which would be an uninitialized instance.
    /// </summary>
    readonly bool _isInitialized;

    /// <summary>
    /// Indicates whether the result represents a successful operation. It is true if the result is a success, and false if it
    /// is a failure.
    /// </summary>
    public bool IsSuccess => _isInitialized
                                ? _error is null
                                : throw new InvalidOperationException("Result is not initialized. default(Result<T>) is illegal.");

    /// <summary>
    /// Indicates whether the result represents a failed operation. It is true if the result is a failure, and false if it
    /// is a success.
    /// </summary>
    public bool IsFailure => _isInitialized
                                ? _error is not null
                                : throw new InvalidOperationException("Result is not initialized. default(Result<T>) is illegal.");

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
        _isInitialized = true;
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
    Result(Error error)
    {
        _error = error;
        _isInitialized = true;
    }

    /// <summary>
    /// Creates a new Result instance representing a successful operation that returns the specified value.
    /// </summary>
    /// <param name="value">
    /// The value of the result. This parameter must not be null.
    /// </param>
    public static Result<T> Ok(T value) => new(value);

    /// <summary>
    /// Creates a new Result instance representing a failed operation with the specified error.
    /// </summary>
    /// <param name="e">
    /// The error that caused the result to be a failure. This parameter must not be null.
    /// </param>
    public static Result<T> Fail([NotNull] Error e)
    {
        ArgumentNullException.ThrowIfNull(e);
        return new(e);
    }

    /// <summary>
    /// Gets the value associated with a successful Result. If the Result represents a failure, accessing this property will throw
    /// an InvalidOperationException.
    /// </summary>
    public T Value => IsSuccess
                            ? _value!
                            : throw new InvalidOperationException("Result is a failure; it has no Value.");

    /// <summary>
    /// Gets the error associated with a failed Result. If the Result represents a success, accessing this property will throw
    /// an InvalidOperationException.
    /// </summary>
    public Error Error => IsFailure
                            ? _error! // IsFailure already tested _error is not null
                            : throw new InvalidOperationException("Result is a success; it has no Error.");
}
