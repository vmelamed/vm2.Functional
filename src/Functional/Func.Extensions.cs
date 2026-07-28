// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Provides extension methods for functions.
/// </summary>
public static class FuncExtensions
{
    /// <summary>
    /// Converts an <see cref="Action{T}"/> to a <see cref="Func{T, Unit}"/> that returns <see cref="Unit.Instance"/>.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter for the action and for the resulting function.</typeparam>
    /// <param name="action">The action to be converted into a function.</param>
    /// <returns>
    /// A function that takes an input of type <typeparamref name="T"/> and returns <see cref="Unit.Instance"/>.
    /// </returns>
    public static Func<T, Unit> ToFunc<T>(this Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return x =>
        {
            action(x);
            return Unit.Instance;
        };
    }

    /// <summary>
    /// Composes two functions, where the output of the first function becomes the input of the second function.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter for the first function.</typeparam>
    /// <typeparam name="RIntermediate">The output of the first function and the input of the second function.</typeparam>
    /// <typeparam name="R">The output of the second function and the output of the composed function.</typeparam>
    /// <param name="f1">The first function to be composed.</param>
    /// <param name="f2">The second function to be composed.</param>
    /// <returns>A function that represents the composition of <paramref name="f1"/> and <paramref name="f2"/>.</returns>
    public static Func<T, R> Compose<T, RIntermediate, R>(
        this Func<T, RIntermediate> f1,
        Func<RIntermediate, R> f2)
    {
        ArgumentNullException.ThrowIfNull(f1);
        ArgumentNullException.ThrowIfNull(f2);

        return x => f2(f1(x));
    }

    /// <summary>
    /// Partially applies the first argument of a function with two parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
    /// <typeparam name="R">The return type of the function.</typeparam>
    /// <param name="f">The function to partially apply.</param>
    /// <param name="t1">The value to apply to the first parameter of the function.</param>
    /// <returns>A function that takes the remaining parameter and returns the result.</returns>
    public static Func<T2, R> Apply<T1, T2, R>(this Func<T1, T2, R> f, T1 t1)
    {
        ArgumentNullException.ThrowIfNull(f);

        return t2 => f(t1, t2);  // t1 comes from the closure
    }

    /// <summary>
    /// Partially applies the first argument of a function with three parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
    /// <typeparam name="R">The return type of the function.</typeparam>
    /// <param name="f">The function to partially apply.</param>
    /// <param name="t1">The value to apply to the first parameter of the function.</param>
    /// <returns>A function that takes the remaining parameters and returns the result.</returns>
    public static Func<T2, T3, R> Apply<T1, T2, T3, R>(this Func<T1, T2, T3, R> f, T1 t1)
    {
        ArgumentNullException.ThrowIfNull(f);

        return (t2, t3) => f(t1, t2, t3);    // t1 comes from the closure
    }

    /// <summary>
    /// Partially applies the first argument of a function with four parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
    /// <typeparam name="R">The return type of the function.</typeparam>
    /// <param name="f">The function to partially apply.</param>
    /// <param name="t1">The value to apply to the first parameter of the function.</param>
    /// <returns>A function that takes the remaining parameters and returns the result.</returns>
    public static Func<T2, T3, T4, R> Apply<T1, T2, T3, T4, R>(this Func<T1, T2, T3, T4, R> f, T1 t1)
    {
        ArgumentNullException.ThrowIfNull(f);

        return (t2, t3, t4) => f(t1, t2, t3, t4);  // t1 comes from the closure
    }

    // TODO: add more overloads for functions with more parameters, if necessary.

    /// <summary>
    /// Curries a function with two parameters: converting a function with N parameters into a chain of functions each taking a
    /// single parameter from the original function in order they appear.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
    /// <typeparam name="R">The return type of the function.</typeparam>
    /// <param name="f">The function to curry.</param>
    /// <returns>The curried version of the function.</returns>
    public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(this Func<T1, T2, R> f)
    {
        ArgumentNullException.ThrowIfNull(f);

        return t1 => t2 => f(t1, t2);
    }

    /// <summary>
    /// Curries a function with two parameters: converting a function with N parameters into a chain of functions each taking a
    /// single parameter from the original function in order they appear.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
    /// <typeparam name="R">The return type of the function.</typeparam>
    /// <param name="f">The function to curry.</param>
    /// <returns>The curried version of the function.</returns>
    public static Func<T1, Func<T2, Func<T3, R>>> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> f)
    {
        ArgumentNullException.ThrowIfNull(f);

        return t1 => t2 => t3 => f(t1, t2, t3);
    }

    /// <summary>
    /// Curries a function with two parameters: converting a function with N parameters into a chain of functions each taking a
    /// single parameter from the original function in order they appear.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
    /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
    /// <typeparam name="R">The return type of the function.</typeparam>
    /// <param name="f">The function to curry.</param>
    /// <returns>The curried version of the function.</returns>
    public static Func<T1, Func<T2, Func<T3, Func<T4, R>>>> Curry<T1, T2, T3, T4, R>(this Func<T1, T2, T3, T4, R> f)
    {
        ArgumentNullException.ThrowIfNull(f);

        return t1 => t2 => t3 => t4 => f(t1, t2, t3, t4);
    }

    // TODO: add more overloads for functions with more parameters, if necessary.
}
