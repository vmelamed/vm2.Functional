// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents the result of an operation that can be in one of two states or cases:
/// <list type="bullet">
/// <item><b>success</b>: represents the outcome of a successful operation that returned value of type <typeparamref name="T"/></item>
/// <item><b>failure</b>: represents the outcome of a failed operation explained by an <see cref="Functional.Error"/> instance</item>
/// </list>
/// </summary>
/// <typeparam name="T">The type of the value returned by a successful operation. MUST NOT be:
/// <list type="bullet">
/// <item>nullable reference type, e.g., <c>string?</c>. Use <c>Option&lt;string&gt;</c> instead</item>
/// <item>nullable value type, e.g. <c>int?</c>. Use <c>Option&lt;int&gt;</c> instead</item>
/// <item><see cref="void"/>. Use <see cref="Unit"/> instead.</item>
/// <item><see cref="Functional.Error"/></item>
/// </list>
/// </typeparam>
/// <remarks>
/// Known elsewhere as <c>Either</c> (Haskell/Scala, where "failure" is <c>Left</c> and "success" is <c>Right</c>) or
/// <c>Result</c> (F#/Rust, where "failure" is <c>Error</c>/<c>Err</c>). This library pins the failure side to
/// <see cref="Functional.Error"/>, so a <c>Result&lt;T&gt;</c> is effectively an <c>Either&lt;Error, T&gt;</c>.
/// <para/>
/// If the type <typeparamref name="T"/> is legitimately nullable, use <c>Result&lt;Option&lt;T&gt;&gt;</c>.
/// <para/>
/// When the operation carries only side effects and has no meaningful return value, use <see cref="Unit"/> as the type parameter.
/// <para/>
/// If the <typeparamref name="T"/> is <see cref="Functional.Error"/> the implicit type-cast operators — <see cref="Result{T}(T)"/>
/// and <see cref="Result{T}(Error)"/> apply, and the compiler reports <b>CS0457 "Ambiguous user defined conversions"</b>. This
/// is a safe failure mode - a compile error, not a silently wrong conversion. So Result&lt;Error&gt; is simply an unsupported
/// instantiation (does not compile) rather than a latent trap.
/// </remarks>
public readonly struct Result<T> : IEquatable<Result<T>> where T : notnull
{
    #region Fields
    /// <summary>
    /// The value associated with a successful <see cref="Result{T}"/>, otherwise it is intentionally <see langword="null"/> and
    /// it is not used.
    /// </summary>
    readonly T? _value;

    /// <summary>
    /// The error associated with a failed <see cref="Result{T}"/>. If the <see cref="Result{T}"/> represents a success, this
    /// field is intentionally <see langword="null"/> and it is not used. If the <see cref="Result{T}"/> represents a failure
    /// and the error is not set, and it will be replaced by <see cref="DefaultError.Instance"/> on the fly -
    /// <see cref="Functional.Error"/>.
    /// </summary>
    readonly Error? _error;

#pragma warning disable IDE0032 // Use auto property.
    // We cannot use auto property -- it would allow for the following bug:
    // var ok = Result<int>.Ok(5);
    // var broken = ok with { IsSuccess = false };
    // where the state of the Result<T> structure (_value: 5, _error: <see langword="null"/>, _isSuccess: false) - would be invalid.

    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    readonly bool _isSuccess;
#pragma warning restore IDE0032 // Use auto property
    #endregion

    #region Properties
    /// <summary>
    /// Indicates whether the result represents a successful operation.
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Indicates whether the result represents a failed operation.
    /// </summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Gets the value associated with a successful <see cref="Result{T}"/>. If the <see cref="Result{T}"/> represents a failure,
    /// accessing this property will throw an <see cref="InvalidOperationException"/> - a bug that needs to be fixed ASAP.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to access the Value property of a failed <see cref="Result{T}"/>.
    /// </exception>
    public T Value => _isSuccess
                            ? _value!
                            : throw new InvalidOperationException("Result is a failure; it has no Value.");

    /// <summary>
    /// Gets the error associated with a failed <see cref="Result{T}"/>. If the <see cref="Result{T}"/> represents a success,
    /// accessing this property will throw an <see cref="InvalidOperationException"/> - a bug that needs to be fixed ASAP.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to access the Error property of a successful <see cref="Result{T}"/>.
    /// </exception>
    public Error Error => !_isSuccess
                                ? _error ?? DefaultError.Instance
                                : throw new InvalidOperationException("Result is a success; it has no Error.");
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> representing a "success" result with the specified value.
    /// </summary>
    /// <param name="value">The value to be encapsulated in the "success" result.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided value is <see langword="null"/>.</exception>
    /// <remarks>
    /// Note that this constructor is private. Use the implicit conversion operator that takes <typeparamref name="T"/> to
    /// create "success" <see cref="Result{T}"/> instances, or use the function <see cref="Ok{T}(T)"/>.
    /// </remarks>
    /// <example>
    /// To create a "successful" result:
    /// <code><![CDATA[
    /// Result<int> Answer() => 42; // implicitly cast to "success" Result&lt;int&gt;
    /// // alternatively:
    /// Result<int> Answer() => Ok(42);
    /// ]]></code></example>
    Result(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        _isSuccess = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> representing a "failure" result with the specified <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The error instance to be encapsulated in the "failure" result.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="error"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Note that this constructor is private. Use the implicit conversion operator that takes an <see cref="Error"/> to create
    /// "failure" <see cref="Result{T}"/> instance, or use the function <see cref="Fail{T}(Error)"/>.
    /// </remarks>
    /// <example>
    /// To create a "failure" result:
    /// <code><![CDATA[
    ///     Result<int> Bad() => SomeError; // implicitly cast SomeError to "failure" Result<int>
    /// // alternatively:
    ///     Result<int> Bad() => Fail(SomeError);
    /// ]]></code></example>
    Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        _error = error;
        _isSuccess = false;
    }
    #endregion

    #region Implicit type-casting operators
    /// <summary>
    /// Creates an instance of <see cref="Result{T}"/> containing the specified value.
    /// </summary>
    /// <param name="value">The value of the result. The cast value MUST NOT be <see langword="null"/>.</param>
    /// <example><code><![CDATA[
    /// Result<int> UltimateQuestion() => 42; // Implicitly creates a successful <see cref="Result<T>"/> with the value 42.
    /// ]]></code></example>
    public static implicit operator Result<T>(T value) => new(value);

    /// <summary>
    /// Creates an instance of <see cref="Result{T}"/> containing the specified error.
    /// </summary>
    /// <param name="error">
    /// The error that caused the result to be a failure. This parameter MUST NOT be <see langword="null"/>.
    /// </param>
    public static implicit operator Result<T>(Error error) => new(error);
    #endregion

    #region Functions - methods typical in FP (Functional Programming)
    /// <summary>
    /// Transforms the value of a "success" <see cref="Result{T}"/> to an <typeparamref name="R"/> value by invoking
    /// <paramref name="onSuccess"/>; or invokes <paramref name="onFailure"/> for a "failure" <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="R">The result type. May differ from <typeparamref name="T"/>.</typeparam>
    /// <param name="onSuccess">A function invoked with the contained value of a "success" <see cref="Result{T}"/>.</param>
    /// <param name="onFailure">A function invoked with the contained error of a "failure" <see cref="Result{T}"/>.</param>
    /// <returns>
    /// The result of invoking either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.
    /// </returns>
    /// <remarks>
    /// Known elsewhere as <c>fold</c>/<c>cata</c> (Haskell), <c>match</c> (F#/Rust pattern matching), <c>either</c>
    /// (for <c>Either</c>).
    /// </remarks>
    public R Match<R>(Func<T, R> onSuccess, Func<Error, R> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess
                    ? onSuccess(_value!)
                    : onFailure(_error ?? DefaultError.Instance);
    }

    /// <summary>
    /// Projects the contained <typeparamref name="T"/> value to an <typeparamref name="R"/> value using the (total) function
    /// <paramref name="selector"/>. Stays inside the same container - <c>Result&lt;&gt;</c>:
    /// <list type="bullet"><item>
    /// "success" <c>Result&lt;R&gt;</c> is mapped to a "success" <c>Result&lt;R&gt;</c>
    /// </item><item>
    /// the <see cref="Error"/> of a "failure" <c>Result&lt;T&gt;</c> is preserved in a "failure" <c>Result&lt;R&gt;</c>.
    /// </item></list>
    /// </summary>
    /// <typeparam name="R">The type of the mapped value.</typeparam>
    /// <param name="selector">
    /// The mapping function applied to the internal value of a "success" <see cref="Result{T}"/>, projecting it to an
    /// <typeparamref name="R"/> value. This parameter MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns><list type="bullet"><item>
    /// A "success" <c>Result&lt;R&gt;</c> instance representing a successful result of type <typeparamref name="R"/>, when the
    /// original <see cref="Result{T}"/> is "success"
    /// </item><item>
    /// A "failure" <c>Result&lt;R&gt;</c> with the error of the original "failure" <see cref="Result{T}"/>
    /// </item></list></returns>
    /// <remarks>
    /// Known elsewhere as <c>fmap</c>/<c>&lt;$&gt;</c> (Haskell), <c>map</c> (F#/Scala/Rust), <c>Project</c> (general).
    /// The .NET LINQ alias <see cref="Result.Select{T,R}(Result{T}, Func{T, R})"/> forwards here, so the query syntax works.
    /// </remarks>
    public Result<R> Map<R>(Func<T, R> selector) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);

        return _isSuccess
                    ? new Result<R>(selector(_value!))
                    : new Result<R>(_error ?? DefaultError.Instance);
    }

    /// <summary>
    /// Chains a partial, result-returning function: applies <paramref name="selector"/> to the contained value and returns its
    /// <c>Result&lt;R&gt;</c> result directly. Like <see cref="Map{R}"/> it stays in the same container - <c>Result&lt;&gt;</c>.
    /// However, unlike <see cref="Map{R}"/>, the result is not re-wrapped, so the chained calls stay <b>flat</b>
    /// (no <c>Result&lt;Result&lt;...&gt;&gt;</c>). Also, the function may be partial - it may return "failure" for a "success"
    /// input.
    /// </summary>
    /// <typeparam name="R">
    /// The type of the encapsulated <c>R</c> value in the instance returned by <paramref name="selector"/> and
    /// <see cref="Result{T}.Bind"/>. May differ from <typeparamref name="T"/>. MUST NOT be nullable.
    /// </typeparam>
    /// <param name="selector">
    /// The partial, result-returning function applied to the contained value when the result is "success". It may return a
    /// "failure" <c>Result&lt;R&gt;</c> even for a "success" input. MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <c>Result&lt;R&gt;</c> computed by <c>f(value)</c> when this is "success"; otherwise "failure" <c>Result&lt;R&gt;</c>.
    /// </returns>
    /// <remarks>
    /// Known elsewhere as <c>&gt;&gt;=</c> (Haskell), <c>bind</c> (F#), <c>flatMap</c> (Scala/Java), <c>chain</c> (JS).
    /// The .NET LINQ alias <see cref="Result.SelectMany{T,R}(Result{T}, Func{T, Result{R}})"/> forwards here so the LINQ method
    /// syntax works.
    /// See also the .NET LINQ method <seealso cref="Enumerable.SelectMany{T,R}(IEnumerable{T}, Func{T, IEnumerable{R}})"/>.
    /// Note that the LINQ expression syntax would never generate a call to either of these methods - it needs a method with a
    /// signature similar to:
    /// <see cref="Enumerable.SelectMany{TSource,TCollection,TResult}(IEnumerable{TSource}, Func{TSource,IEnumerable{TCollection}}, Func{TSource,TCollection,TResult})"/>.
    /// This type of method is provided in the static class <see cref="Result"/>.
    /// </remarks>
    public Result<R> Bind<R>(Func<T, Result<R>> selector) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);

        return _isSuccess
                    ? selector(_value!)
                    : new Result<R>(_error ?? DefaultError.Instance);
    }

    /// <summary>
    /// Returns the original <see cref="Result{T}"/> unchanged if it is a success and the <paramref name="predicate"/> returns
    /// <see langword="true"/>; otherwise, a failed <see cref="Result{T}"/> with a specified error.
    /// </summary>
    /// <param name="predicate">
    /// The predicate to evaluate for the contained value when the result is a success. This parameter MUST NOT be
    /// <see langword="null"/>.
    /// </param>
    /// <param name="error">
    /// The error to use for the failed <see cref="Result{T}"/> when the predicate returns <see langword="false"/>. This
    /// parameter MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns><list type="bullet"><item>
    /// The original <see cref="Result{T}"/> if it is "success" and the predicate is <see langword="true"/>, or if the
    /// <see cref="Result{T}"/> is "failure"
    /// </item><item>
    /// A new "failure" <see cref="Result{T}"/> with the specified <paramref name="error"/> if it is "success" and the predicate
    /// is <see langword="false"/>
    /// </item></list></returns>
    /// <remarks>
    /// A predicate guard on the success track. Known elsewhere as <c>ensure</c>/<c>guard</c> (validation combinators);
    /// there is no direct F#/LINQ built-in (LINQ's <c>Where</c> cannot supply an error).
    /// </remarks>
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
    /// Executes <paramref name="onSuccess"/> for the contained value when the result is "success", and returns the original
    /// result unchanged.
    /// </summary>
    /// <param name="onSuccess">
    /// The action to execute for the contained value when the result is a success. MUST NOT be <see langword="null"/>.
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
    /// Executes <paramref name="onSuccess"/> for the contained value when the result is a success, or <paramref name="onFailure"/>
    /// when the result is a failure, and returns the original result unchanged.
    /// </summary>
    /// <param name="onSuccess">
    /// The action to execute for the contained value when the result is a success. MUST NOT be <see langword="null"/>.
    /// </param>
    /// <param name="onFailure">
    /// The action to execute for the contained error when the result is a failure. MUST NOT be <see langword="null"/>.
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

    /// <summary>
    /// Gets the value of a "success" <see cref="Result{T}"/>, or returns the specified <paramref name="fallback"/> value if the
    /// <see cref="Result{T}"/> is "failure".
    /// </summary>
    /// <param name="fallback">
    /// The fallback value to return if the <see cref="Result{T}"/> represents a failure. This parameter MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The value of the "success" <see cref="Result{T}"/>, or the specified <paramref name="fallback"/> value if the
    /// <see cref="Result{T}"/> is "failure".
    /// </returns>
    /// <remarks>
    /// Known elsewhere as <c>getOrElse</c> (Scala), <c>defaultValue</c> (F#), <c>unwrap_or</c> (Rust).
    /// </remarks>
    public T GetValueOr(T fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        return _isSuccess ? _value! : fallback;
    }
    #endregion

    #region .NET identity
    /// <inheritdoc />
    public bool Equals(Result<T> other)
        => _isSuccess
                ? other._isSuccess && EqualityComparer<T>.Default.Equals(_value, other._value)
                : !other._isSuccess && EqualityComparer<Error>.Default.Equals(Error, other.Error);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Result<T> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(
                typeof(Result<T>).GetHashCode(),
                _isSuccess.GetHashCode(),
                _isSuccess
                    ? (_value?.GetHashCode() ?? 0)
                    : (_error?.GetHashCode() ?? DefaultError.Instance.GetHashCode()));

    /// <inheritdoc />
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);
    #endregion
}
