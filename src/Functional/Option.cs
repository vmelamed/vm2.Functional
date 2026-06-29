// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents an optional value. An instance of <see cref="Option{T}"/> can be in one of two states: it can either contain a
/// value of type <typeparamref name="T"/> (the "some" case) or it can represent the absence of a value (the "none" case).
/// </summary>
/// <typeparam name="T">The type encapsulated by </typeparam>
public readonly struct Option<T> : IEquatable<NoneType>, IEquatable<Option<T>>
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
        _value = value ?? throw new ArgumentNullException(nameof(value), "Option cannot contain null value. Use None instead.");
        _isSome = true;
    }

    /// Creates an instance of <see cref="Option{T}"/> containing the specified value.
    /// <param name="value">The value to be encapsulated by the created <see cref="Option{T}"/> instance.</param>
    /// <returns>An instance of <see cref="Option{T}"/> containing the specified value.</returns>
    public static implicit operator Option<T>(T value) => new(value);

    /// Creates an instance of <see cref="Option{T}"/> representing the absence of a value.
    /// <param name="_">The parameter is ignored.</param>
    /// <returns>An instance of <see cref="Option{T}"/> representing the absence of a value.</returns>
    public static implicit operator Option<T>(NoneType _) => default;

    /// Creates an instance of <see cref="Option{T}"/> representing the absence of a value.
    /// <param name="_">The parameter is ignored. It is only used to distinguish this method from the implicit conversion
    /// operator that creates an instance of <see cref="Option{T}"/> containing a value.
    /// </param>
    /// <returns>An instance of <see cref="Option{T}"/> representing the absence of a value.</returns>
    public static implicit operator NoneType(Option<T> _) => None;

    /// <summary>
    /// Maps the current <see cref="Option{T}"/> instance to a value of type <typeparamref name="R"/> using the specified mapping functions.
    /// </summary>
    /// <typeparam name="R">
    /// The type of the value returned by the mapping functions. This type can be different from <typeparamref name="T"/>.
    /// </typeparam>
    /// <param name="onSome">
    /// The function to be called if the current <see cref="Option{T}"/> instance contains a value.
    /// </param>
    /// <param name="onNone">
    /// The function to be called if the current <see cref="Option{T}"/> instance represents the absence of a value.
    /// </param>
    /// <returns>
    /// The result of calling either <paramref name="onSome"/> with the contained value (if the current <see cref="Option{T}"/>
    /// instance contains a value) or <paramref name="onNone"/> (if the current <see cref="Option{T}"/> instance represents the
    /// absence of a value).
    /// </returns>
    public R Match<R>(Func<T, R> onSome, Func<R> onNone)
        => _isSome ? onSome(_value) : onNone();

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
        => _isSome && other._isSome && EqualityComparer<T>.Default.Equals(_value, other._value);

    /// <summary>
    /// Determines whether the current <see cref="Option{T}"/> instance is equal to an object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with the current <see cref="Option{T}"/> instance. This object can be either an instance of
    /// <see cref="NoneType"/> or an instance of <see cref="Option{T}"/>. If the object is of any other type, the method returns
    /// <see langword="false"/>.
    /// </param>
    /// <returns></returns>
    public override bool Equals(object? obj) => obj is NoneType none ? Equals(none) : obj is Option<T> option && Equals(option);

    /// <summary>
    /// Returns a hash code for the current <see cref="Option{T}"/> instance. The hash code is computed based on the state of
    /// the instance: if the instance is in the "some" state, the hash code of the contained value is returned; if the instance
    /// is in the "none" state, a constant hash code (0) is returned.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="Option{T}"/> instance.
    /// </returns>
    public override int GetHashCode()
        => HashCode.Combine(typeof(Option<T>).GetHashCode(), _isSome ? _value?.GetHashCode() ?? 0 : 0);
}
