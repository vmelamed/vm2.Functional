// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Provides static helper methods for creating <see cref="Result{T}"/> and <see cref="Result{Unit}"/> instances,
/// both for successful and failed results.
/// </summary>
public static class Result
{
    /// <summary>
    /// Static helper method for creating <see cref="Result{Unit}"/> instances representing a successful result with no value.
    /// </summary>
    public static Result<Unit> Ok() => Result<Unit>.Ok(default);

    /// <summary>
    /// Static helper method for creating <see cref="Result{Unit}"/> instances representing a failed result with the specified error.
    /// </summary>
    public static Result<Unit> Fail(Error error) => Result<Unit>.Fail(error);

    /// <summary>
    /// Static helper method for creating <see cref="Result{T}"/> instances representing a successful result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value for the Result instance.</typeparam>
    /// <returns><see cref="Result{T}"/> instance representing a successful result with the specified value.</returns>
    public static Result<T> Ok<T>(T value) where T : notnull => Result<T>.Ok(value);

    /// <summary>
    /// Static helper method for creating <see cref="Result{T}"/> instances representing a failed result with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value for the Result instance.</typeparam>
    /// <param name="error">The error that caused the result to be a failure. This parameter must not be null.</param>
    /// <returns><see cref="Result{T}"/> instance representing a failed result with the specified error.</returns>
    public static Result<T> Fail<T>(Error error) where T : notnull => Result<T>.Fail(error);
}
