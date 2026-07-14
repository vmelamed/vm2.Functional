// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents a result of an operation that can either be a success or a failure, and carries a value of the result of type T
/// in case of success.
/// <para/>
/// Note that <typeparamref name="T"/> must be a non-nullable type, i.e. void must be represented by <see cref="Unit"/> and
/// <see cref="string"/>? is not allowed. Functional means no <see langword="null"/>-s.
/// <para/>
/// Also note that <see cref="Error"/> is not allowed either: when <typeparamref name="T"/> is <see cref="Error"/> both implicit
/// cast operators — <see cref="Result{T}(T)"/> and <see cref="Result{T}(Error)"/> apply, and the compiler reports <b>CS0457
/// "Ambiguous user defined conversions"</b>. This is the safe failure mode (a compile error, not a silently wrong conversion), so
/// Result&lt;Error&gt; is simply an unsupported instantiation rather than a latent trap.
/// </summary>
/// <typeparam name="T">
/// The type of the value carried by the Result in case of a successful operation. MUST NOT be <see cref="Error"/> - see the
/// remarks below.
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
#pragma warning disable IDE0032 // Use auto property. No! It introduces a bug:
    // var broken = ok with { IsSuccess = false };   // _value=5, _error=null, IsSuccess=false
    readonly bool _isSuccess;
#pragma warning restore IDE0032 // Use auto property

    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Indicates whether the result represents a failed operation.
    /// </summary>
    public bool IsFailure => !IsSuccess;

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
        ArgumentNullException.ThrowIfNull(value);

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
    Result(Error error)
    {
        _error = error;
        _isSuccess = false;
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
    public static Result<T> Fail(Error e) => new(e);

    /// <summary>
    /// Gets the value associated with a successful Result. If the Result represents a failure, accessing this property will throw
    /// an InvalidOperationException.
    /// </summary>
    public T Value => _isSuccess
                        ? _value!
                        : throw new InvalidOperationException("Result is a failure; it has no Value.");

    /// <summary>
    /// Gets the error associated with a failed Result. If the Result represents a success, accessing this property will throw
    /// an InvalidOperationException.
    /// </summary>
    public Error Error => !_isSuccess
                            ? _error ?? DefaultError.Instance
                            : throw new InvalidOperationException("Result is a success; it has no Error.");

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

    /// <summary>
    /// Unwraps the <see cref="Result{T}"/> into a value of type <typeparamref name="R"/> by invoking
    /// <paramref name="onSuccess"/> for the "Value" case; or <paramref name="onFailure"/> for the "Error" case.
    /// </summary>
    /// <typeparam name="R">The result type. May differ from <typeparamref name="T"/>.</typeparam>
    /// <param name="onSuccess">Invoked with the contained value when the result is "Value".</param>
    /// <param name="onFailure">Invoked when the result is "Error".</param>
    /// <returns>
    /// The result of invoking either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.
    /// </returns>
    public R Match<R>(Func<T, R> onSuccess, Func<Error, R> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
                ? onSuccess(_value!)
                : onFailure(_error ?? DefaultError.Instance);
    }

    /// <summary>
    /// Transforms the value of a successful Result&lt;T&gt; into a value of type R using the specified mapping function or
    /// returns a failed Result&lt;R&gt; with the same error if the original Result is a failure.
    /// </summary>
    /// <typeparam name="R">
    /// The type of the value for the resulting Result instance. This type must be non-nullable.
    /// </typeparam>
    /// <param name="f">
    /// The mapping function to transform the value of a successful Result&lt;T&gt; into a value of type R. This parameter must not be null.
    /// </param>
    /// <returns>
    /// A Result&lt;R&gt; instance representing a successful result with the transformed value if the original Result is a
    /// success, or a failed Result&lt;R&gt; with the same error if the original Result is a failure.
    /// </returns>
    public Result<R> Map<R>(Func<T, R> f) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(f);

        return _isSuccess
                ? new Result<R>(f(_value!))
                : new Result<R>(_error ?? DefaultError.Instance);
    }

    /// <summary>
    /// Transforms the value of a successful Result&lt;T&gt; into a Result&lt;R&gt; using the specified binding function or
    /// returns a failed Result&lt;R&gt; with the same error if the original Result is a failure.
    /// </summary>
    /// <typeparam name="R">
    /// The type of the value for the resulting Result instance. This type must be non-nullable.
    /// </typeparam>
    /// <param name="f">
    /// The binding function to transform the value of a successful Result&lt;T&gt; into a Result&lt;R&gt;.
    /// This parameter must not be null.
    /// </param>
    /// <returns>
    /// A Result&lt;R&gt; instance representing a successful result with the transformed value if the original Result is a
    /// success, or a failed Result&lt;R&gt; with the same error if the original Result is a failure.
    /// </returns>
    public Result<R> Bind<R>(Func<T, Result<R>> f) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(f);

        return _isSuccess
                ? f(_value!)
                : new Result<R>(_error ?? DefaultError.Instance);
    }

    /// <summary>
    /// If the this Result is a success, and the predicate returns true, the original Result is returned unchanged.
    /// If the predicate returns false, a failed Result with the specified error is returned.
    /// </summary>
    /// <param name="predicate">
    /// The predicate to evaluate for the contained value when the result is a success. This parameter must not be null.
    /// </param>
    /// <param name="error">
    /// The error to use for the failed Result when the predicate returns false. This parameter must not be null.
    /// </param>
    /// <returns>
    /// The original Result unchanged if it is a success and the predicate returns true; otherwise, a failed Result with the
    /// specified error.
    /// </returns>
    public Result<T> Ensure(Func<T, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        if (IsFailure)
            return this;

        return predicate(_value!)
                ? this
                : new Result<T>(error);
    }

    /// <summary>
    /// Gets the value of a successful Result, or returns the specified fallback value if the Result represents a failure.
    /// </summary>
    /// <param name="fallback">
    /// The fallback value to return if the Result represents a failure. This parameter must not be null.
    /// </param>
    /// <returns>
    /// The value of a successful Result, or the specified fallback value if the Result represents a failure.
    /// </returns>
    public T GetValueOr(T fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        return _isSuccess ? _value! : fallback;
    }

    /// <summary>
    /// Executes <paramref name="onSuccess"/> for the contained value when the result is a success, and returns the original
    /// result unchanged.
    /// </summary>
    /// <param name="onSuccess">
    /// The action to execute for the contained value when the result is a success. This parameter must not be null.
    /// </param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// This is useful for performing side effects (e.g., logging) without affecting the result's value.
    /// </remarks>
    public Result<T> Tap(Action<T> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);

        if (_isSuccess)
            onSuccess(_value!);
        return this;
    }

    /// <summary>
    /// Executes <paramref name="onSuccess"/> for the contained value when the result is a success, or
    /// <paramref name="onFailure"/> when the result is a failure, and returns the original result unchanged.
    /// </summary>
    /// <param name="onSuccess">
    /// The action to execute for the contained value when the result is a success. This parameter must not be null.
    /// </param>
    /// <param name="onFailure">
    /// The action to execute when the result is a failure. This parameter must not be null.
    /// </param>
    /// <returns>
    /// The original result unchanged.
    /// </returns>
    public Result<T> Tap(Action<T> onSuccess, Action<Error> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (_isSuccess)
            onSuccess(_value!);
        else
            onFailure(_error ?? DefaultError.Instance);
        return this;
    }

}
