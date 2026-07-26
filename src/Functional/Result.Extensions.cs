// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Static helpers and extension methods for <see cref="Result{T}"/>.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates an instance of "success" <see cref="Result{T}"/> containing the specified value. This method is provided as a
    /// more explicit alternative to the implicit conversion operator that takes a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the encapsulated value contained in the created <see cref="Result{T}"/> instance.</typeparam>
    /// <param name="value">The value for the "success" <see cref="Result{T}"/> instance. MUST NOT be <see langword="null"/>.</param>
    /// <returns>A "success" <see cref="Result{T}"/> with the specified value.</returns>
    /// <remarks>
    /// The success constructor. Known elsewhere as <c>return</c>/<c>pure</c> (Haskell, the monadic unit),
    /// <c>Ok</c> (F#/Rust native), <c>Right</c> (Haskell/Scala, for <c>Either</c>).
    /// </remarks>
    public static Result<T> Ok<T>(T value) where T : notnull
        => value;   // impl.conversion to Result<T>

    /// <summary>
    /// Creates an instance of "failure" <see cref="Result{T}"/> containing the failure reason as an <see cref="Error"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value of the "failure" instance.</typeparam>
    /// <param name="error">The error that caused the failure. MUST NOT be <see langword="null"/>.</param>
    /// <returns><see cref="Result{T}"/> instance representing a failed result with the specified error.</returns>
    /// <remarks>
    /// The failure constructor. Known elsewhere as <c>Error</c>/<c>Err</c> (F#/Rust), <c>Left</c> (Haskell/Scala,
    /// for <c>Either</c>).
    /// </remarks>
    public static Result<T> Fail<T>(Error error) where T : notnull
        => error;   // impl.conversion of Error to Result<T>

    /// <summary>
    /// Creates <see cref="Result{Unit}"/> instances representing a successful outcome with no value.
    /// </summary>
    /// <returns><see cref="Result{Unit}"/> instance representing a successful outcome with no value.</returns>
    public static Result<Unit> Ok()
        => Unit.Instance;   // impl.conversion of Unit to Result<Unit>

    /// <summary>
    /// Creates <see cref="Result{Unit}"/> instances representing a failed outcome with the specified error.
    /// </summary>
    /// <returns><see cref="Result{Unit}"/> instance representing a failed outcome with the specified error.</returns>
    /// <remarks>
    /// Leverages the implicit conversion of <see cref="Error"/> values to create a failed result.
    /// </remarks>
    public static Result<Unit> Fail(Error error)
        => error;   // impl.conversion of Error to Result<Unit>

    extension<T>(Result<T> result) where T : notnull
    {
        /// <summary>
        /// Projects the contained <typeparamref name="T"/> value to an <typeparamref name="R"/> value using the total function
        /// <paramref name="selector"/>. Stays inside the same container - <c>Result&lt;&gt;</c>:
        /// <list type="bullet"><item>
        /// "success" <c>Result&lt;R&gt;</c> is mapped or projected to a "success" <c>Result&lt;R&gt;</c>
        /// </item><item>
        /// the <see cref="Error"/> of a "failure" <c>Result&lt;T&gt;</c> is preserved in a "failure" <c>Result&lt;R&gt;</c>.
        /// </item></list>
        /// </summary>
        /// <typeparam name="R">The type of the mapped value.</typeparam>
        /// <param name="selector">
        /// The mapping function applied to the internal value of a "success" <see cref="Result{T}"/>, projecting it to an
        /// <typeparamref name="R"/> value. MUST NOT be <see langword="null"/>.
        /// </param>
        /// <returns><list type="bullet"><item>
        /// A "success" <c>Result&lt;R&gt;</c> instance representing a successful result of type <typeparamref name="R"/>, when the
        /// original <see cref="Result{T}"/> is "success"
        /// </item><item>
        /// A "failure" <c>Result&lt;R&gt;</c> with the error of the original "failure" <see cref="Result{T}"/>
        /// </item></list></returns>
        /// <remarks>
        /// This function is a synonym for <see cref="Result{T}.Map{R}(Func{T, R})"/>. It bridges the vocabulary of
        /// this library to the LINQ vocabulary - <seealso cref="Enumerable.Select{T,R}(IEnumerable{T}, Func{T,R})"/>
        /// </remarks>
        public Result<R> Select<R>(Func<T, R> selector) where R : notnull
            => result.Map(selector);

        /// <summary>
        /// Chains a partial, result-returning function: applies <paramref name="selector"/> to the contained value and returns its
        /// <c>Result&lt;R&gt;</c> result directly. Like <see cref="Select"/> it stays in the same container - <c>Result&lt;&gt;</c>.
        /// However, unlike <see cref="Select"/>, the result is not re-wrapped, so the chained calls stay <b>flat</b>
        /// (no <c>Result&lt;Result&lt;...&gt;&gt;</c>). Also the mapping function is partial - it may return "failure" for
        /// "success" inputs.
        /// </summary>
        /// <typeparam name="R">
        /// The type of the encapsulated <c>R</c> value in the instance returned by <paramref name="selector"/> and
        /// <see cref="Result{T}.Bind"/>. May differ from <typeparamref name="T"/>. MUST NOT be nullable.
        /// </typeparam>
        /// <param name="selector">
        /// The partial, result-returning function applied to the contained value when the result is "success". It may return a
        /// "failure" <c>Result&lt;R&gt;</c> when the result is "success". MUST NOT be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <c>Result&lt;R&gt;</c> computed by <c>f(value)</c> when this is "success"; otherwise "failure" <c>Result&lt;R&gt;</c>.
        /// </returns>
        /// <remarks>
        /// This function is a synonym for <see cref="Result{T}.Bind{R}(Func{T, Result{R}})"/>. It bridges the vocabulary of
        /// this library to the LINQ vocabulary - <see cref="Enumerable.SelectMany{T,R}(IEnumerable{T}, Func{T, IEnumerable{R}})"/>.
        /// </remarks>
        public Result<R> SelectMany<R>(Func<T, Result<R>> selector) where R : notnull
            => result.Bind(selector);

        /// <summary>
        /// Supports LINQ query syntax with multiple <c>from</c> clauses over <see cref="Result{T}"/>. The compiler
        /// binds this overload for a query such as
        /// <c>from t in optT from r in optR select f(t, r)</c>: <paramref name="selector"/> supplies the second
        /// result from the first's value, and <paramref name="resultSelector"/> combines both values into the result.
        /// It is the query-syntax alias of <see cref="Result{T}.Bind{R}(Func{T, Result{R}})"/> followed by a
        /// projection — you would not normally call it directly.
        /// </summary>
        /// <typeparam name="R">The value type produced by <paramref name="selector"/> (the second <c>from</c> source).</typeparam>
        /// <typeparam name="C">The final result type produced by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="selector">
        /// Produces the second <c>Result&lt;R&gt;</c> from the value of the first - <c>Result&lt;T&gt;</c>. Invoked only when
        /// the first result is "success"; a "failure" at either step short-circuits the whole query to "failure".
        /// </param>
        /// <param name="resultSelector">
        /// Combines the internal value <c>t</c> of the first <c>Result&lt;T&gt;</c> and the internal value <c>r</c> of the
        /// second <c>Result&lt;R&gt;</c> into the final result wrapped in <c>Result&lt;C&gt;</c>. Invoked only when the second
        /// result is also "success"; a "failure" at either step short-circuits the whole query to "failure".
        /// </param>
        /// <returns>
        /// <c>Ok(resultSelector(t, r))</c> when both results are "success"; otherwise the propagated "failure".
        /// </returns>
        /// <remarks>
        /// This is the two-argument <c>SelectMany</c> shape the C# query-expression pattern requires — it is not a plain
        /// rename of <see cref="Result{T}.Bind{R}(Func{T, Result{R}})"/>. The <paramref name="resultSelector"/> exists so the
        /// outer bound value (<c>t</c>) remains available to the final <c>select</c>. Prefer <c>Bind</c>/<c>Map</c> directly in
        /// method syntax.
        /// </remarks>
        public Result<C> SelectMany<R, C>(
            Func<T, Result<R>> selector,
            Func<T, R, C> resultSelector)
            where R : notnull
            where C : notnull
            => result.Bind(
                        t => selector(t)
                                .Map(r => resultSelector(t, r)));
    }
}
