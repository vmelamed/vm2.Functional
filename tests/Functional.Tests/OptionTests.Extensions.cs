// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class OptionTests
{
    #region Some / None factories
    [Fact]
    public void Some_WhenValueProvided_ShouldBeSome()
    {
        Option<int> option = Some(42);

        option.Match(v => v, () => -1).Should().Be(42);
    }

    [Fact]
    public void Some_WhenReferenceValueProvided_ShouldBeSome()
    {
        Option<string> option = Some("forty two");

        option.Match(v => v, () => "none").Should().Be("forty two");
    }

    [Fact]
    public void None_WhenAssigned_ShouldBeNone()
    {
        Option<int> option = None;

        option.Equals(None).Should().BeTrue();
    }
    #endregion

    #region Select (LINQ alias of Map)
    [Fact]
    public void Select_WhenSome_ShouldProjectValue()
    {
        Option<int> option = Some(42);

        var projected = from x in option select x + 1;

        projected.Match(v => v, () => -1).Should().Be(43);
    }

    [Fact]
    public void Select_WhenNone_ShouldStayNone()
    {
        Option<int> option = None;
        var called = false;

        var projected = option.Select(x => { called = true; return x + 1; });

        projected.Equals(None).Should().BeTrue();
        called.Should().BeFalse();
    }
    #endregion

    #region SelectMany — one argument (LINQ alias of Bind)
    [Fact]
    public void SelectMany_WhenSome_ShouldChainAndFlatten()
    {
        Option<int> option = Some(42);

        var chained = option.SelectMany(x => Some(x + 1));

        chained.Match(v => v, () => -1).Should().Be(43);
    }

    [Fact]
    public void SelectMany_WhenSomeButSelectorReturnsNone_ShouldBeNone()
    {
        Option<int> option = Some(42);

        var chained = option.SelectMany(_ => (Option<int>)None);

        chained.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void SelectMany_WhenNone_ShouldShortCircuit()
    {
        Option<int> option = None;
        var called = false;

        var chained = option.SelectMany(x => { called = true; return Some(x + 1); });

        chained.Equals(None).Should().BeTrue();
        called.Should().BeFalse();
    }
    #endregion

    #region SelectMany — two arguments (query syntax with multiple 'from')
    [Fact]
    public void SelectMany_WhenBothSome_ShouldCombineWithResultSelector()
    {
        Option<int> first = Some(42);
        Option<int> second = Some(8);

        var combined =
            from a in first
            from b in second
            select a + b;

        combined.Match(v => v, () => -1).Should().Be(50);
    }

    [Fact]
    public void SelectMany_WhenFirstNone_ShouldShortCircuitBeforeSecond()
    {
        Option<int> first = None;
        var secondEvaluated = false;

        var combined =
            from a in first
            from b in Evaluate()
            select a + b;

        combined.Equals(None).Should().BeTrue();
        secondEvaluated.Should().BeFalse();

        Option<int> Evaluate() { secondEvaluated = true; return Some(8); }
    }

    [Fact]
    public void SelectMany_WhenSecondNone_ShouldBeNone()
    {
        Option<int> first = Some(42);
        Option<int> second = None;

        var combined =
            from a in first
            from b in second
            select a + b;

        combined.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void SelectMany_WhenTypeChanging_ShouldThreadOuterValueToResultSelector()
    {
        // Crosses the type (int -> string) and uses the outer bound value 'a' in the final select,
        // exercising the reason resultSelector exists.
        Option<int> first = Some(42);

        var combined =
            from a in first
            from b in Some($"n{a}")
            select $"{a}:{b}";

        combined.Match(v => v, () => "none").Should().Be("42:n42");
    }
    #endregion

    #region Where (LINQ alias of Filter)
    [Fact]
    public void Where_WhenSomeAndPredicateHolds_ShouldStaySome()
    {
        Option<int> option = Some(42);

        var filtered = from x in option where x > 0 select x;

        filtered.Match(v => v, () => -1).Should().Be(42);
    }

    [Fact]
    public void Where_WhenSomeAndPredicateFails_ShouldBeNone()
    {
        Option<int> option = Some(42);

        var filtered = option.Where(x => x < 0);

        filtered.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Where_WhenNone_ShouldStayNoneWithoutEvaluatingPredicate()
    {
        Option<int> option = None;
        var called = false;

        var filtered = option.Where(_ => { called = true; return true; });

        filtered.Equals(None).Should().BeTrue();
        called.Should().BeFalse();
    }
    #endregion
}
