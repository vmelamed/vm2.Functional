// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class OptionTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    #region Construction and conversion
    [Fact]
    public void ImplicitFromValue_WhenValueProvided_ShouldBeSome()
    {
        Option<int> option = 42;

        // A "some" is not equal to None; Match proves it takes the onSome branch.
        option.Equals(None).Should().BeFalse();
        option.Match(v => v, () => -1).Should().Be(42);
    }

    [Fact]
    public void ImplicitFromNone_WhenAssigned_ShouldBeNone()
    {
        Option<int> option = None;

        option.Equals(None).Should().BeTrue();
        option.Match(_ => true, () => false).Should().BeFalse();
    }

    [Fact]
    public void Default_ShouldBeNone()
    {
        // default(Option<T>) must be the "none" state — the whole point of storing the success flag.
        var option = default(Option<int>);

        option.Equals(None).Should().BeTrue();
        option.Match(_ => true, () => false).Should().BeFalse();
    }

    [Fact]
    public void ImplicitFromValue_WhenStringValueIsNull_ShouldThrow()
    {
        // The private ctor's null guard fires through the implicit conversion.
        var act = () => { Option<string> _ = null!; };

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }
    #endregion

    #region Match
    [Fact]
    public void Match_WhenSome_ShouldInvokeOnSomeWithValue()
    {
        Option<int> option = 7;

        var result = option.Match(onSome: v => $"some:{v}", onNone: () => "none");

        result.Should().Be("some:7");
    }

    [Fact]
    public void Match_WhenNone_ShouldInvokeOnNone()
    {
        Option<int> option = None;

        var result = option.Match(onSome: v => $"some:{v}", onNone: () => "none");

        result.Should().Be("none");
    }
    #endregion

    #region Map
    [Fact]
    public void Map_WhenSome_ShouldProjectValueAndStaySome()
    {
        Option<int> option = 5;

        var mapped = option.Map(v => v * 2);

        mapped.Equals(None).Should().BeFalse();
        mapped.Match(v => v, () => -1).Should().Be(10);
    }

    [Fact]
    public void Map_WhenNone_ShouldStayNone()
    {
        Option<int> option = None;

        var mapped = option.Map(v => v * 2);

        mapped.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Map_WhenNone_ShouldNotInvokeTheProjection()
    {
        var invoked = false;
        Option<int> option = None;

        _ = option.Map(v => { invoked = true; return v * 2; });

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Map_WhenSome_ShouldChangeResultType()
    {
        Option<int> option = 7;

        Option<string> mapped = option.Map(v => $"n:{v}");

        mapped.Match(s => s, () => "none").Should().Be("n:7");
    }
    #endregion

    #region Bind
    // Two option-returning helpers used to prove chaining stays flat and short-circuits on None.
    static Option<int> Halve(int n) => n % 2 == 0 ? n / 2 : None;
    static Option<string> Label(int n) => $"={n}";

    [Fact]
    public void Bind_WhenSome_ShouldApplyFunctionAndStayFlat()
    {
        Option<int> option = 8;

        // The result is Option<int>, NOT Option<Option<int>> — Bind does not re-wrap.
        Option<int> bound = option.Bind(Halve);

        bound.Match(v => v, () => -1).Should().Be(4);
    }

    [Fact]
    public void Bind_WhenSomeButFunctionReturnsNone_ShouldBeNone()
    {
        Option<int> option = 7; // odd -> Halve returns None

        var bound = option.Bind(Halve);

        bound.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Bind_WhenNone_ShouldStayNoneAndNotInvokeFunction()
    {
        var invoked = false;
        Option<int> option = None;

        var bound = option.Bind(v => { invoked = true; return Halve(v); });

        bound.Equals(None).Should().BeTrue();
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Bind_WhenChained_ShouldThreadThroughAllSteps()
    {
        Option<int> option = 8;

        // 8 -> Halve -> 4 -> Halve -> 2 -> Label -> "=2"; every step stays single-level.
        var result = option.Bind(Halve).Bind(Halve).Bind(Label);

        result.Match(s => s, () => "none").Should().Be("=2");
    }

    [Fact]
    public void Bind_WhenChainShortCircuits_ShouldPropagateNone()
    {
        Option<int> option = 8;

        // 8 -> 4 -> 2 -> 1, then Halve(1) is None (1 is odd) -> Label never runs -> None all the way out.
        var result = option.Bind(Halve).Bind(Halve).Bind(Halve).Bind(Halve).Bind(Label);

        result.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Bind_WhenSome_InvokesFunctionOnceWithUnwrappedValue()
    {
        var f = Substitute.For<Func<int, Option<int>>>();
        f.Invoke(Arg.Any<int>()).Returns(ci => Some(ci.Arg<int>() * 2));

        Some(21).Bind(f).Should().Be(Some(42));
        f.Received(1).Invoke(21);
    }

    [Fact]
    public void Bind_WhenNone_DoesNotInvokeFunctionWithUnwrappedValue()
    {
        var f = Substitute.For<Func<int, Option<int>>>();
        f.Invoke(Arg.Any<int>()).Returns(ci => Some(ci.Arg<int>() * 2));
        Option<int> option = None;

        option.Bind(f).Should().Be(None);
        f.Received(0).Invoke(21);
    }
    #endregion

    #region Tap
    [Fact]
    public void Tap_WhenSome_ShouldRunOnSomeWithValueAndReturnSameOption()
    {
        var seen = 0;
        Option<int> option = 5;

        var returned = option.Tap(v => seen = v);

        seen.Should().Be(5);
        returned.Should().Be(option); // returns the original option for chaining
    }

    [Fact]
    public void Tap_WhenNone_ShouldNotRunOnSomeAndReturnNone()
    {
        var ran = false;
        Option<int> option = None;

        var returned = option.Tap(_ => ran = true);

        ran.Should().BeFalse();
        returned.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Tap_WhenSome_ShouldRunOnSomeNotOnNone()
    {
        var someRan = false;
        var noneRan = false;
        Option<int> option = 5;

        option.Tap(onSome: _ => someRan = true, onNone: () => noneRan = true);

        someRan.Should().BeTrue();
        noneRan.Should().BeFalse();
    }

    [Fact]
    public void Tap_WhenNone_ShouldRunOnNoneNotOnSome()
    {
        var someRan = false;
        var noneRan = false;
        Option<int> option = None;

        option.Tap(onSome: _ => someRan = true, onNone: () => noneRan = true);

        someRan.Should().BeFalse();
        noneRan.Should().BeTrue();
    }

    [Fact]
    public void Tap_WhenChained_ShouldReturnSameOptionAtEachStep()
    {
        Option<int> option = 5;

        var result = option.Tap(_ => { }).Tap(_ => { });

        result.Should().Be(option);
    }

    [Fact]
    public void Tap_WhenOnSomeIsNull_ShouldThrow()
    {
        Option<int> option = 5;

        var act = () => option.Tap(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onSome");
    }

    [Fact]
    public void Tap_WhenOnNoneIsNull_ShouldThrow()
    {
        Option<int> option = 5;

        var act = () => option.Tap(_ => { }, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onNone");
    }
    #endregion

    #region Filter
    [Fact]
    public void Filter_WhenSomeAndPredicateTrue_ShouldReturnSameOption()
    {
        Option<int> option = 6;

        var filtered = option.Filter(v => v % 2 == 0);

        filtered.Should().Be(option); // unchanged
    }

    [Fact]
    public void Filter_WhenSomeAndPredicateFalse_ShouldBecomeNone()
    {
        Option<int> option = 7;

        var filtered = option.Filter(v => v % 2 == 0);

        filtered.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Filter_WhenNone_ShouldStayNone()
    {
        Option<int> option = None;

        var filtered = option.Filter(v => v % 2 == 0);

        filtered.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Filter_WhenNone_ShouldNotInvokePredicate()
    {
        var invoked = false;
        Option<int> option = None;

        _ = option.Filter(v => { invoked = true; return true; });

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Filter_WhenPredicateIsNull_ShouldThrow()
    {
        Option<int> option = 5;

        var act = () => option.Filter(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("predicate");
    }
    #endregion

    #region GetValueOr
    [Fact]
    public void GetValueOr_WhenSome_ShouldReturnContainedValue()
    {
        Option<int> option = 5;

        option.GetValueOr(99).Should().Be(5);
    }

    [Fact]
    public void GetValueOr_WhenNone_ShouldReturnFallback()
    {
        Option<int> option = None;

        option.GetValueOr(99).Should().Be(99);
    }

    [Fact]
    public void GetValueOr_WhenFallbackIsNull_ShouldThrow()
    {
        Option<string> option = None;

        var act = () => option.GetValueOr(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("fallback");
    }
    #endregion

    #region ToOption — nullable value type
    [Fact]
    public void ToOption_WhenNullableValueTypeHasValue_ShouldBeSome()
    {
        int? nullable = 42;

        var option = nullable.ToOption();

        option.Match(v => v, () => -1).Should().Be(42);
    }

    [Fact]
    public void ToOption_WhenNullableValueTypeIsNull_ShouldBeNone()
    {
        int? nullable = null;

        var option = nullable.ToOption();

        option.Equals(None).Should().BeTrue();
    }
    #endregion

    #region ToOption — nullable reference type
    [Fact]
    public void ToOption_WhenReferenceIsNonNull_ShouldBeSome()
    {
        string? reference = "hello";

        var option = reference.ToOption();

        option.Match(s => s, () => "none").Should().Be("hello");
    }

    [Fact]
    public void ToOption_WhenReferenceIsNull_ShouldBeNone()
    {
        string? reference = null;

        var option = reference.ToOption();

        option.Equals(None).Should().BeTrue();
    }
    #endregion
}
