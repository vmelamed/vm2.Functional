// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents an optional value. An instance of <c>Option&lt;T&gt;</c> can be in one of two states or cases:
/// <list type="bullet">
/// <item><b>some</b>: represents a presence of a value of type <typeparamref name="T"/></item>
/// <item><b>none</b>: represents the absence of a value</item>
/// </list>
/// </summary>
/// <typeparam name="T">The type of the value encapsulated by this <see langword="struct"/> when the value is present.</typeparam>
/// <remarks>
/// Known elsewhere as <c>Maybe</c> (Haskell/Elm), <c>Option</c> (F#/Scala/Rust), <c>Optional</c> (Java/Swift).
/// <para/>
/// Note that <typeparamref name="T"/> MUST be a non-nullable reference or value type. I.e. <see cref="string"/>? or
/// <see cref="int"/>? are not valid type parameters. Use the function <see cref="Option.ToOption{T}(T?)"/> to convert
/// potentially nullable values to <see cref="Option{T}"/> values instead.
/// </remarks>
public readonly struct Option<T> : IEquatable<NoneType>, IEquatable<Option<T>> where T : notnull
{
    #region Fields
    /// <summary>
    /// The value encapsulated by the option when it is in the "some" state.
    /// </summary>
    readonly T _value;

    /// <summary>
    /// When <see langword="true"/>, indicates that the option is in the "some" state and the <see cref="_value"/> is valid;
    /// otherwise, it is in the "none" state and the <see cref="_value"/> should not be accessed.
    /// </summary>
    readonly bool _isSome;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="Option{T}"/> struct with the specified value.
    /// </summary>
    /// <param name="value">
    /// The value to be encapsulated by the created <see cref="Option{T}"/> instance. This value MUST NOT be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Note that this constructor is private. Use the implicit conversion operators that take <typeparamref name="T"/> or a
    /// <see cref="NoneType"/> (e.g. <see cref="None"/>) values to create instances of <see cref="Option{T}"/> instead.
    /// </remarks>
    /// <example>
    /// To create an option in a "some" state:
    /// <code>
    ///     Option&lt;int&gt; answer1 = 42;                     // implicitly invokes the operator Option&lt;int&gt;
    ///     Option&lt;int&gt; answer2 = Some(42);               // Some() invokes the implicit operator internally
    /// </code>
    /// To create an option in a "none" state:
    /// <code>
    ///     Option&lt;int&gt; noneValue2 = Option.None;         // the same as above
    ///     Option&lt;int&gt; noneValue3 = default;             // directly assigns the default Option&lt;int&gt; value, representing "none"
    /// </code></example>
    Option(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        _isSome = true;
    }
    #endregion

    #region Implicit type-casting operators
    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> containing the <paramref name="value"/>. This is the only way to
    /// create a "some" instance and used by <see cref="Option.Some{T}(T)"/>.
    /// </summary>
    /// <param name="value">
    /// The value to be encapsulated by the created <see cref="Option{T}"/> instance. The cast value MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns>An instance of <see cref="Option{T}"/> containing the specified value.</returns>
    public static implicit operator Option<T>(T value) => new(value);

    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> representing the absence of a <typeparamref name="T"/> value. This is the
    /// only public way to create a "none" instance - <see cref="None"/>.
    /// </summary>
    /// <param name="_">The parameter is ignored.</param>
    /// <returns>An instance of <see cref="Option{T}"/> representing the absence of a value.</returns>
    public static implicit operator Option<T>(NoneType _) => default;
    #endregion

    #region Functions - methods typical in FP (Functional Programming)
    /// <summary>
    /// Transforms the value of a "some" <see cref="Option{T}"/> to an <typeparamref name="R"/> value by invoking
    /// <paramref name="onSome"/>; or invokes <paramref name="onNone"/> for a "none" <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="R">The result type. May differ from <typeparamref name="T"/>.</typeparam>
    /// <param name="onSome">Invoked with the contained value when the option is "some". MUST NOT be <see langword="null"/>.</param>
    /// <param name="onNone">Invoked when the option is "none". MUST NOT be <see langword="null"/>.</param>
    /// <returns>The result of <paramref name="onSome"/> or <paramref name="onNone"/>, whichever applies.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="onSome"/> or <paramref name="onNone"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Known elsewhere as <c>fold</c>/<c>cata</c> (Haskell), <c>match</c> (F#/Rust pattern matching), <c>maybe</c>
    /// (Haskell, for <c>Maybe</c>).
    /// </remarks>
    public R Match<R>(Func<T, R> onSome, Func<R> onNone)
    {
        ArgumentNullException.ThrowIfNull(onSome);
        ArgumentNullException.ThrowIfNull(onNone);

        return _isSome
                ? onSome(_value)
                : onNone();
    }

    /// <summary>
    /// Projects the contained <typeparamref name="T"/> value to an <typeparamref name="R"/> value using the total function
    /// <paramref name="selector"/>. Stays inside the same container - <c>Option&lt;&gt;</c>:
    /// <list type="bullet"><item>
    /// "some" <see cref="Option{T}"/> is mapped to a "some" <c>Option&lt;R&gt;</c>;
    /// </item><item>
    /// "none" <see cref="Option{T}"/> is preserved as "none" <c>Option&lt;R&gt;</c>.
    /// </item></list>
    /// </summary>
    /// <typeparam name="R">The type of the mapped value.</typeparam>
    /// <param name="selector">
    /// The mapping function applied to the internal value of a "some" <see cref="Option{T}"/>, projecting it to an
    /// <typeparamref name="R"/> value. MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns><list type="bullet"><item>
    /// A "some" <c>Option&lt;R&gt;</c> instance representing a successful result of type <typeparamref name="R"/>, when the
    /// original <see cref="Option{T}"/> is "some"
    /// </item><item>
    /// A "none" <c>Option&lt;R&gt;</c> when the original <see cref="Option{T}"/> is "none"
    /// </item></list></returns>
    /// <remarks>
    /// Known elsewhere as <c>fmap</c>/<c>&lt;$&gt;</c> (Haskell), <c>map</c> (F#/Scala/Rust), <c>Project</c> (general).
    /// The .NET LINQ alias <see cref="Option.Select{T,R}(Option{T}, Func{T, R})"/> forwards here so query syntax works.
    /// </remarks>
    public Option<R> Map<R>(Func<T, R> selector) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);

        return _isSome
                    ? new Option<R>(selector(_value))
                    : default;
    }

    /// <summary>
    /// Chains a partial, option-returning function: applies <paramref name="selector"/> to the contained value and returns its
    /// <c>Option&lt;R&gt;</c> result directly. Like <see cref="Map{R}"/> it stays in the same container - <c>Option&lt;&gt;</c>.
    /// However, unlike <see cref="Map{R}"/>, the result is not re-wrapped, so the chained calls stay <b>flat</b> (no
    /// <c>Option&lt;Option&lt;...&gt;&gt;</c>). Also, the mapping function may be partial - it may return "none" for "some"
    /// inputs.
    /// </summary>
    /// <typeparam name="R">
    /// The type of the encapsulated <c>R</c> value in the instance returned by <paramref name="selector"/> and
    /// <see cref="Option{T}.Bind"/>. May differ from <typeparamref name="T"/>. MUST NOT be nullable.
    /// </typeparam>
    /// <param name="selector">
    /// The partial, option-returning function applied to the contained value when the option is "some". It may return a "none"
    /// <c>Option&lt;R&gt;</c> when the option is "some". MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <c>Option&lt;R&gt;</c> computed by <c>f(value)</c> when this is "some"; otherwise "none" <c>Option&lt;R&gt;</c>.
    /// </returns>
    /// <remarks>
    /// Known elsewhere as <c>&gt;&gt;=</c> (Haskell), <c>bind</c> (F#), <c>flatMap</c> (Scala/Java), <c>chain</c> (JS).
    /// The .NET LINQ alias <see cref="Option.SelectMany{T,R}(Option{T}, Func{T, Option{R}})"/> forwards here so the LINQ method
    /// syntax works.
    /// See also the .NET LINQ method <seealso cref="Enumerable.SelectMany{T,R}(IEnumerable{T}, Func{T, IEnumerable{R}})"/>.
    /// Note that the LINQ expression syntax would never generate a call to either of these methods - it needs a method with a
    /// signature similar to:
    /// <see cref="Enumerable.SelectMany{TSource,TCollection,TResult}(IEnumerable{TSource}, Func{TSource,IEnumerable{TCollection}}, Func{TSource,TCollection,TResult})"/>.
    /// This type of method is provided in the static class <see cref="Option"/>.
    /// </remarks>
    public Option<R> Bind<R>(Func<T, Option<R>> selector) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);

        return _isSome
                    ? selector(_value)
                    : default;
    }

    /// <summary>
    /// Executes <paramref name="onSome"/> for the contained value when the option is "some" or it does nothing when the option
    /// is "none".
    /// </summary>
    /// <param name="onSome">The action to execute for the contained value when the option is "some". MUST NOT be <see langword="null"/>.</param>
    /// <returns>The original option unchanged.</returns>
    /// <remarks>
    /// This is useful for performing side effects (e.g., logging) without affecting the option's value.
    /// </remarks>
    /// <seealso cref="Tap(Action{T}, Action)"/>
    public Option<T> Tap(Action<T> onSome)
    {
        ArgumentNullException.ThrowIfNull(onSome);

        if (_isSome)
            onSome(_value);
        return this;
    }

    /// <summary>
    /// Executes <paramref name="onSome"/> for the contained value when the option is "some" or <paramref name="onNone"/> when
    /// the option is "none".
    /// </summary>
    /// <param name="onSome">The action to execute for the contained value when the option is "some". MUST NOT be <see langword="null"/>.</param>
    /// <param name="onNone">The action to execute when the option is "none". MUST NOT be <see langword="null"/>.</param>
    /// <returns>The original option unchanged.</returns>
    /// <remarks>
    /// This is useful for performing side effects (e.g., logging) without affecting the option's value.
    /// </remarks>
    /// <seealso cref="Tap(Action{T})"/>
    public Option<T> Tap(Action<T> onSome, Action onNone)
    {
        ArgumentNullException.ThrowIfNull(onSome);
        ArgumentNullException.ThrowIfNull(onNone);

        if (_isSome)
            onSome(_value);
        else
            onNone();
        return this;
    }

    /// <summary>
    /// Preserves the "some" option, if the contained value satisfies the <paramref name="predicate"/>; otherwise, returns "none".
    /// </summary>
    /// <param name="predicate">The predicate to test the contained value when the option is "some". MUST NOT be <see langword="null"/>.</param>
    /// <returns>
    /// The original option if it is "some" and the contained value satisfies the predicate; otherwise, "none".
    /// </returns>
    /// <remarks>
    /// Known elsewhere as <c>filter</c> (Haskell/F#/Scala), <c>Where</c> (LINQ).
    /// </remarks>
    public Option<T> Filter(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _isSome && predicate(_value)
                ? this
                : default;
    }

    /// <summary>
    /// Returns the contained value if the option is "some"; otherwise, returns the <paramref name="fallback"/>.
    /// </summary>
    /// <param name="fallback">
    /// The value to return when the option is "none". This value MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The contained value if the option is "some"; otherwise, the specified <paramref name="fallback"/>.
    /// </returns>
    /// <remarks>
    /// Known elsewhere as <c>getOrElse</c> (Scala), <c>defaultValue</c> (F#), <c>unwrap_or</c> (Rust).
    /// </remarks>
    public T GetValueOr(T fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        return _isSome ? _value : fallback;
    }
    #endregion

    #region .NET identity
    /// <summary>
    /// Determines whether the current <see cref="Option{T}"/> instance is equal to an instance of <see cref="NoneType"/>.
    /// </summary>
    /// <param name="_">
    /// The <see cref="NoneType"/> instance to compare with the current <see cref="Option{T}"/> instance.
    /// This parameter is ignored, as the equality is determined solely by the state of the current <see cref="Option{T}"/>
    /// instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the current <see cref="Option{T}"/> instance represents the absence of a value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(NoneType _) => !_isSome;

    /// <summary>
    /// Determines whether the current <see cref="Option{T}"/> instance is equal to another instance of <see cref="Option{T}"/>.
    /// </summary>
    /// <param name="other">The <see cref="Option{T}"/> instance to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true"/> if both instances are in the "some" state and their contained values are equal according to the
    /// default equality comparer for type <typeparamref name="T"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Option<T> other)
        => _isSome
                ? other._isSome && EqualityComparer<T>.Default.Equals(_value, other._value)
                : !other._isSome;

    /// <summary>
    /// Determines whether the current <see cref="Option{T}"/> instance is equal to an object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with the current <see cref="Option{T}"/> instance. This object can be either an instance of
    /// <see cref="NoneType"/> or an instance of <see cref="Option{T}"/>. If the object is of any other type, the method returns
    /// <see langword="false"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the current <see cref="Option{T}"/> instance is equal to the specified object; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is NoneType none
            ? Equals(none)
            : obj is Option<T> option &&
                Equals(option);

    /// <summary>
    /// Returns a hash code for the current <see cref="Option{T}"/> instance. The hash code is computed based on the state of
    /// the instance: if the instance is in the "some" state, the hash code of the contained value is returned; if the instance
    /// is in the "none" state, a constant hash code (0) is returned.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="Option{T}"/> instance.
    /// </returns>
    public override int GetHashCode()
        => HashCode.Combine(
                typeof(Option<T>).GetHashCode(),
                _isSome
                    ? _value?.GetHashCode() ?? 0
                    : 0);

    /// <summary>
    /// Determines whether two <see cref="Option{T}"/> instances are equal. Two instances are considered equal if they are both
    /// in the "some" state and their contained values are equal according to the default equality comparer for type
    /// <typeparamref name="T"/> or if they are both in the "none" state.
    /// </summary>
    /// <param name="left">
    /// The first <see cref="Option{T}"/> instance to compare.
    /// </param>
    /// <param name="right">
    /// The second <see cref="Option{T}"/> instance to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the two <see cref="Option{T}"/> instances are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Option{T}"/> instances are not equal. Two instances are considered not equal if they
    /// are in different states (one is in the "some" state and the other is in the "none" state) or if they are both in the
    /// "some" state but their contained values are not equal.
    /// </summary>
    /// <param name="left">
    /// The first <see cref="Option{T}"/> instance to compare.
    /// </param>
    /// <param name="right">
    /// The second <see cref="Option{T}"/> instance to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the two <see cref="Option{T}"/> instances are not equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);
    #endregion
}
