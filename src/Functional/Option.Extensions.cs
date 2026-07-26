// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Static helpers and extension methods for <see cref="Option{T}"/>.
/// </summary>
public static class Option
{
    /// <summary>
    /// Creates an instance of "some" <see cref="Option{T}"/> containing the specified value. This method is provided as a more
    /// explicit alternative to the implicit conversion operator that takes a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the encapsulated value contained by the created <see cref="Option{T}"/> instance.</typeparam>
    /// <param name="value">The value for the "some" <see cref="Option{T}"/> instance. MUST NOT be <see langword="null"/>.</param>
    /// <returns>A "some" <see cref="Option{T}"/> containing the specified value.</returns>
    /// <remarks>
    /// Known elsewhere as <c>return</c>/<c>pure</c> (Haskell, the monadic unit), <c>Some</c> (F#/Rust native),
    /// <c>Just</c> (Haskell, for <c>Maybe</c>).
    /// </remarks>
    /// <example>
    /// To create an option in a "some" state:
    /// <code>
    ///     Option&lt;int&gt; answer1 = Some(42);               // Some() invokes the implicit operator internally
    ///     Option&lt;int&gt; answer2 = 42;                     // implicitly invokes the operator Option&lt;int&gt;
    /// </code></example>
    public static Option<T> Some<T>(T value) where T : notnull
        => value;   // implicit conversion to Option<T>

    /// <summary>
    /// Creates an instance of <see cref="Option{T}"/> representing the absence of a value.
    /// </summary>
    /// <example>
    /// To create an option in a "none" state:
    /// <code>
    ///     Option&lt;int&gt; noneValue1 = Option.None;         // using this property
    ///     Option&lt;int&gt; noneValue3 = default;             // directly assigns the default Option&lt;int&gt; value, representing "none"
    /// </code></example>
    public static NoneType None
        => default; // implicit conversion of NoneType to Option<T>

    /// <summary>Extension methods for converting nullable value types to <see cref="Option{T}"/>.</summary>
    /// <typeparam name="T">The underlying value type of the nullable value to be converted.</typeparam>
    /// <param name="t">The value to be converted to an <see cref="Option{T}"/>. Can be <see langword="null"/>.</param>
    extension<T>(T? t) where T : struct
    {
        /// <summary>Converts nullable value type <see cref="Nullable{T}"/> (e.g. <c>int?</c>) to an <see cref="Option{T}"/>.</summary>
        /// <returns>
        /// An instance of <see cref="Option{T}"/> representing "some" with the specified value, if it is non-null; otherwise,
        /// an instance of <see cref="Option{T}"/> representing "none".
        /// </returns>
        public Option<T> ToOption() => t.HasValue ? Some(t.Value) : None;
    }

    /// <summary>
    /// Extension methods for converting reference types to <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The underlying reference type of the value to be converted.
    /// </typeparam>
    /// <param name="t">
    /// The value to be converted to an <see cref="Option{T}"/>. Can be <see langword="null"/>.
    /// </param>
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

    /// <summary>
    /// Extension methods (aliases) bridging the <see cref="Option"/>'s FP function names to the names used in .NET LINQ query.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the <see cref="Option"/> type.</typeparam>
    /// <param name="option">The Option instance to which the LINQ syntax (query or method) will be applied.</param>
    extension<T>(Option<T> option) where T : notnull
    {
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
        /// This function is a synonym for <see cref="Option{T}.Map{R}(Func{T, R})"/>. It bridges the vocabulary of
        /// this library to the LINQ vocabulary - <seealso cref="Enumerable.Select{T,R}(IEnumerable{T}, Func{T,R})"/>
        /// </remarks>
        public Option<R> Select<R>(Func<T, R> selector) where R : notnull
            => option.Map(selector);

        /// <summary>
        /// Chains a partial, option-returning function: applies <paramref name="selector"/> to the contained value and returns its
        /// <c>Option&lt;R&gt;</c> result directly. Like <see cref="Select"/> it stays in the same container - <c>Option&lt;&gt;</c>.
        /// However, unlike <see cref="Select"/>, the result is not re-wrapped, so the chained calls stay <b>flat</b> (no
        /// <c>Option&lt;Option&lt;...&gt;&gt;</c>). Also, the mapping function is partial - it may return "none" for "some"
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
        /// This function is a synonym for <see cref="Option{T}.Bind{R}(Func{T, Option{R}})"/>. It bridges the vocabulary of
        /// this library to the LINQ vocabulary - <see cref="Enumerable.SelectMany{T,R}(IEnumerable{T}, Func{T,IEnumerable{R}})"/>.
        /// </remarks>
        public Option<R> SelectMany<R>(Func<T, Option<R>> selector)
            where R : notnull
            => option.Bind(selector);

        /// <summary>
        /// Supports LINQ query syntax with multiple <c>from</c> clauses over <see cref="Option{T}"/>. The compiler
        /// binds this overload for a query such as
        /// <c>from t in optT from r in optR select f(t, r)</c>: <paramref name="selector"/> supplies the second
        /// option from the first's value, and <paramref name="resultSelector"/> combines both values into the result.
        /// It is the query-syntax alias of <see cref="Option{T}.Bind{R}(Func{T, Option{R}})"/> followed by a
        /// projection — you would not normally call it directly.
        /// </summary>
        /// <typeparam name="R">The value type produced by <paramref name="selector"/> (the second <c>from</c> source).</typeparam>
        /// <typeparam name="C">The final result type produced by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="selector">
        /// Produces the second <c>Option&lt;R&gt;</c> from the value of the first - <c>Option&lt;T&gt;</c>. Invoked only when
        /// the first option is "some"; a "none" at either step short-circuits the whole query to "none".
        /// </param>
        /// <param name="resultSelector">
        /// Combines the internal value <c>t</c> of the first <c>Option&lt;T&gt;</c> and the internal value <c>r</c> of the
        /// second <c>Option&lt;R&gt;</c> into the final result wrapped in <c>Option&lt;C&gt;</c>. Invoked only when the second
        /// option is also "some"; a "none" at either step short-circuits the whole query to "none".
        /// </param>
        /// <returns>
        /// <c>Some(resultSelector(t, r))</c> when both options are "some"; otherwise "none".
        /// </returns>
        /// <remarks>
        /// This is the two-argument <c>SelectMany</c> shape the C# query-expression pattern requires — it is not a plain
        /// rename of <see cref="Option{T}.Bind{R}(Func{T, Option{R}})"/>. The <paramref name="resultSelector"/> exists so the
        /// outer bound value (<c>t</c>) remains available to the final <c>select</c>. Prefer <c>Bind</c>/<c>Map</c> directly in
        /// method syntax.
        /// </remarks>
        public Option<C> SelectMany<R, C>(
            Func<T, Option<R>> selector,
            Func<T, R, C> resultSelector)
            where R : notnull
            where C : notnull
            => option.Bind(
                        t => selector(t)
                                .Map(r => resultSelector(t, r)));

        /// <summary>
        /// Preserves the "some" option, if the contained value satisfies the <paramref name="predicate"/>; otherwise, returns
        /// "none".
        /// </summary>
        /// <param name="predicate">
        /// The predicate to test the contained value when the option is "some". MUST NOT be <see langword="null"/>.
        /// </param>
        /// <returns>
        /// The original option if it is "some" and the contained value satisfies the predicate; otherwise, "none".
        /// </returns>
        public Option<T> Where(Func<T, bool> predicate) => option.Filter(predicate);
    }
}
