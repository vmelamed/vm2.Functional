// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents the absence of value.
/// </summary>
/// <remarks>
/// Replace <see langword="null"/> with <c>default(NoneType)</c> or <see cref="Option.None"/> or just <c>None</c> (requires
/// <c>global using static vm2.Functional.Option;</c>).
/// </remarks>
public readonly record struct NoneType();

/// <summary>
/// Represents an optional value. An instance of <see cref="Option{T}"/> can be in one of two states: it can either contain a
/// value of type <typeparamref name="T"/> (the "some" case) or it can represent the absence of a value (the "none" case).
/// </summary>
/// <typeparam name="T">The type encapsulated by the struct.</typeparam>
public readonly struct Option<T> : IEquatable<NoneType>, IEquatable<Option<T>> where T : notnull
{
    readonly T _value;
    readonly bool _isSome;

    /// <summary>
    /// Initializes a new instance of the <see cref="Option{T}"/> struct containing the specified value.
    /// implicit conversion operator, which enforces the non-null constraint on the value.
    /// </summary>
    /// <param name="value">
    /// The value to be encapsulated by the created <see cref="Option{T}"/> instance.
    /// This value MUST NOT be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// Note that the constructor is private. Use the implicit conversion operators that take <typeparamref name="T"/> and a
    /// <see langword="null"/> values to create an instance of <see cref="Option{T}"/> instead.
    /// </remarks>
    Option(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        _isSome = true;
    }

    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> containing the specified value.
    /// </summary>
    /// <param name="value">The value to be encapsulated by the created <see cref="Option{T}"/> instance.</param>
    /// <returns>An instance of <see cref="Option{T}"/> containing the specified value.</returns>
    public static implicit operator Option<T>(T value) => new(value);

    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> representing the absence of a value.
    /// <param name="_">The parameter is ignored.</param>
    /// </summary>
    /// <returns>An instance of <see cref="Option{T}"/> representing the absence of a value.</returns>
    public static implicit operator Option<T>(NoneType _) => default;

    /// <summary>
    /// Unwraps the <see cref="Option{T}"/> into a value of type <typeparamref name="R"/> by invoking
    /// <paramref name="onSome"/> for the "some" case or <paramref name="onNone"/> for the "none" case.
    /// </summary>
    /// <typeparam name="R">The result type. May differ from <typeparamref name="T"/>.</typeparam>
    /// <param name="onSome">Invoked with the contained value when the option is "some".</param>
    /// <param name="onNone">Invoked when the option is "none".</param>
    /// <returns>The result of <paramref name="onSome"/> or <paramref name="onNone"/>, whichever applies.</returns>
    public R Match<R>(Func<T, R> onSome, Func<R> onNone)
    {
        ArgumentNullException.ThrowIfNull(onSome);
        ArgumentNullException.ThrowIfNull(onNone);

        return _isSome
                ? onSome(_value)
                : onNone();
    }

    /// <summary>
    /// Projects the contained value through <paramref name="f"/>, staying inside <see cref="Option{T}"/>:
    /// "some" is mapped to "some" of the result; "none" is preserved.
    /// </summary>
    /// <typeparam name="R">The mapped value type. May differ from <typeparamref name="T"/>.</typeparam>
    /// <param name="f">The projection applied to the contained value when the option is "some".</param>
    /// <returns><c>Some(f(value))</c> when this is "some"; otherwise "none".</returns>
    public Option<R> Map<R>(Func<T, R> f) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(f);

        return _isSome
                ? new Option<R>(f(_value))
                : default;
    }

    /// <summary>
    /// Executes <paramref name="onSome"/> for the contained value when the option is "some" or it does nothing when the option
    /// is "none".
    /// </summary>
    /// <param name="onSome">
    /// The action to execute for the contained value when the option is "some".
    /// </param>
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
    /// <param name="onSome">
    /// The action to execute for the contained value when the option is "some".
    /// </param>
    /// <param name="onNone">
    /// The action to execute when the option is "none".
    /// </param>
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
    /// Chains an option-returning function: applies <paramref name="f"/> to the contained value and returns its
    /// <see cref="Option{R}"/> result directly. Unlike <see cref="Map{R}"/>, the result is not re-wrapped, so chained
    /// calls stay flat (no <c>Option&lt;Option&lt;...&gt;&gt;</c>).
    /// </summary>
    /// <typeparam name="R">The result value type. May differ from <typeparamref name="T"/>.</typeparam>
    /// <param name="f">The option-returning function applied to the contained value when the option is "some".</param>
    /// <returns><c>f(value)</c> when this is "some"; otherwise "none".</returns>
    public Option<R> Bind<R>(Func<T, Option<R>> f) where R : notnull
    {
        ArgumentNullException.ThrowIfNull(f);

        return _isSome
                ? f(_value)
                : default;
    }

    /// <summary>
    /// Preserves the option if the contained value satisfies a predicate; otherwise, returns "none".
    /// </summary>
    /// <param name="predicate">
    /// The predicate to test the contained value when the option is "some".
    /// </param>
    /// <returns>
    /// The original option if it is "some" and the contained value satisfies the predicate; otherwise, "none".
    /// </returns>
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
    public T GetValueOr(T fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        return _isSome ? _value : fallback;
    }

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
    /// <returns></returns>
    public override bool Equals(object? obj)
        => obj is NoneType
            ? Equals(None)
            : obj is Option<T> option && Equals(option);

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
}

/// <summary>
/// Provides static methods (<see cref="Option.Some{T}(T)"/>) for creating instances of <see cref="Option{T}"/>.
/// </summary>
public static class Option
{
    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> containing the specified value. This method is provided as a more
    /// explicit alternative to the implicit conversion operator that takes a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to be encapsulated by the created <see cref="Option{T}"/> instance.
    /// </typeparam>
    /// <param name="value">
    /// The value to be encapsulated by the created <see cref="Option{T}"/> instance. This value MUST NOT be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An instance of <see cref="Option{T}"/> containing the specified value.
    /// </returns>
    public static Option<T> Some<T>(T value) where T : notnull => value;

    /// <summary>
    /// Constant representing the absence of a value. Can be used wherever an instance of <see cref="Option{T}"/> is expected
    /// leveraging the implicit conversion operator from <see cref="NoneType"/> to <see cref="Option{T}"/>.
    /// </summary>
    public static NoneType None => default;

    extension<T>(T? t) where T : struct
    {
        /// <summary>
        /// Converts a nullable value type (e.g. Nullable{int}) to an <see cref="Option{T}"/>.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="Option{T}"/> representing "some" with the specified value if it is non-null; otherwise,
        /// an instance of <see cref="Option{T}"/> representing "none".
        /// </returns>
        public Option<T> ToOption() => t.HasValue ? Some(t.Value) : None;
    }

    extension<T>(T? t) where T : class
    {
        /// <summary>
        /// Converts a reference type to an <see cref="Option{T}"/>.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="Option{T}"/> representing "some" with the specified reference if it is non-null; otherwise,
        /// an instance of <see cref="Option{T}"/> representing "none".
        /// </returns>
        public Option<T> ToOption() => t is not null ? Some(t) : None;
    }
}
