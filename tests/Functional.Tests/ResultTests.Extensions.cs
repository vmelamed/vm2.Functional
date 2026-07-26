// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class ResultTests
{
    #region Ok / Fail factories
    [Fact]
    public void Ok_WhenValueProvided_ShouldBeSuccess()
    {
        Result<int> result = Ok(42);

        result.Match(v => v, _ => -1).Should().Be(42);
    }

    [Fact]
    public void Fail_WhenErrorProvided_ShouldBeFailure()
    {
        Error error = new TestError("fail", "boom");

        Result<int> result = Fail<int>(error);

        result.Match(_ => (Error?)null, e => e).Should().Be(error);
    }

    [Fact]
    public void OkUnit_ShouldBeSuccessfulUnit()
    {
        Result<Unit> result = Ok();

        result.Match(u => u, _ => throw new InvalidOperationException()).Should().Be(Unit.Instance);
    }

    [Fact]
    public void FailUnit_WhenErrorProvided_ShouldBeFailure()
    {
        Error error = new TestError("fail", "boom");

        Result<Unit> result = Fail(error);

        result.Match(_ => (Error?)null, e => e).Should().Be(error);
    }
    #endregion

    #region Select (LINQ alias of Map)
    [Fact]
    public void Select_WhenSuccess_ShouldProjectValue()
    {
        Result<int> result = Ok(42);

        var projected = from x in result select x + 1;

        projected.Match(v => v, _ => -1).Should().Be(43);
    }

    [Fact]
    public void Select_WhenFailure_ShouldPropagateErrorWithoutProjecting()
    {
        Error error = new TestError("select", "boom");
        Result<int> result = Fail<int>(error);
        var called = false;

        var projected = result.Select(x => { called = true; return x + 1; });

        projected.Match(_ => (Error?)null, e => e).Should().Be(error);
        called.Should().BeFalse();
    }
    #endregion

    #region SelectMany — one argument (LINQ alias of Bind)
    [Fact]
    public void SelectMany_WhenSuccess_ShouldChainAndFlatten()
    {
        Result<int> result = Ok(42);

        var chained = result.SelectMany(x => Ok(x + 1));

        chained.Match(v => v, _ => -1).Should().Be(43);
    }

    [Fact]
    public void SelectMany_WhenSuccessButSelectorFails_ShouldPropagateSelectorError()
    {
        Error error = new TestError("chain", "boom");
        Result<int> result = Ok(42);

        var chained = result.SelectMany(_ => Fail<int>(error));

        chained.Match(_ => (Error?)null, e => e).Should().Be(error);
    }

    [Fact]
    public void SelectMany_WhenFailure_ShouldShortCircuit()
    {
        Error error = new TestError("chain", "boom");
        Result<int> result = Fail<int>(error);
        var called = false;

        var chained = result.SelectMany(x => { called = true; return Ok(x + 1); });

        chained.Match(_ => (Error?)null, e => e).Should().Be(error);
        called.Should().BeFalse();
    }
    #endregion

    #region SelectMany — two arguments (query syntax with multiple 'from')
    [Fact]
    public void SelectMany_WhenBothSuccess_ShouldCombineWithResultSelector()
    {
        Result<int> first = Ok(42);
        Result<int> second = Ok(8);

        var combined =
            from a in first
            from b in second
            select a + b;

        combined.Match(v => v, _ => -1).Should().Be(50);
    }

    [Fact]
    public void SelectMany_WhenFirstFailure_ShouldShortCircuitBeforeSecond()
    {
        Error error = new TestError("first", "boom");
        Result<int> first = Fail<int>(error);
        var secondEvaluated = false;

        var combined =
            from a in first
            from b in Evaluate()
            select a + b;

        combined.Match(_ => (Error?)null, e => e).Should().Be(error);
        secondEvaluated.Should().BeFalse();

        Result<int> Evaluate() { secondEvaluated = true; return Ok(8); }
    }

    [Fact]
    public void SelectMany_WhenSecondFailure_ShouldPropagateSecondError()
    {
        Error error = new TestError("second", "boom");
        Result<int> first = Ok(42);
        Result<int> second = Fail<int>(error);

        var combined =
            from a in first
            from b in second
            select a + b;

        combined.Match(_ => (Error?)null, e => e).Should().Be(error);
    }

    [Fact]
    public void SelectMany_WhenTypeChanging_ShouldThreadOuterValueToResultSelector()
    {
        // Crosses the type (int -> string) and uses the outer bound value 'a' in the final select.
        Result<int> first = Ok(42);

        var combined =
            from a in first
            from b in Ok($"n{a}")
            select $"{a}:{b}";

        combined.Match(v => v, _ => "fail").Should().Be("42:n42");
    }
    #endregion
}
