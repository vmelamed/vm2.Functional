// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public class ResultTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    // A concrete Error for the fixtures — Error itself is abstract, and callers are expected to
    // discriminate by type, not by string-sniffing Code.
    sealed record TestError(string Code, string Message) : Error(Code, Message);

    static readonly TestError SomeError = new("test.failed", "the operation failed");
    static readonly TestError OtherError = new("test.other", "a different failure");

    #region Construction, Ok / Fail, implicit conversions
    [Fact]
    public void Ok_ShouldBeSuccessAndCarryTheValue()
    {
        var result = Result<int>.Ok(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Fail_ShouldBeFailureAndCarryTheError()
    {
        var result = Result<int>.Fail(SomeError);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void ImplicitFromValue_ShouldBeSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitFromError_ShouldBeFailure()
    {
        Result<int> result = SomeError;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Value_WhenFailure_ShouldThrow()
    {
        var result = Result<int>.Fail(SomeError);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Error_WhenSuccess_ShouldThrow()
    {
        var result = Result<int>.Ok(42);

        var act = () => result.Error;

        act.Should().Throw<InvalidOperationException>();
    }
    #endregion

    #region default(Result<T>) — the benign-failure contract
    [Fact]
    public void Default_ShouldBeFailureCarryingDefaultError()
    {
        // A defaulted struct is a *benign failure*, not a landmine: the predicates are total (never throw)
        // and Error yields DefaultError rather than blowing up.
        var result = default(Result<int>);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(DefaultError.Instance);
    }

    [Fact]
    public void Default_Value_ShouldThrow()
    {
        var result = default(Result<int>);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }
    #endregion

    #region Match
    [Fact]
    public void Match_WhenSuccess_ShouldInvokeOnSuccessWithTheValue()
    {
        var result = Result<int>.Ok(7);

        var matched = result.Match(onSuccess: v => $"ok:{v}", onFailure: e => $"err:{e.Code}");

        matched.Should().Be("ok:7");
    }

    [Fact]
    public void Match_WhenFailure_ShouldInvokeOnFailureWithTheError()
    {
        var result = Result<int>.Fail(SomeError);

        // onFailure receives the Error — failure carries information, and Match is where it is consumed.
        var matched = result.Match(onSuccess: v => $"ok:{v}", onFailure: e => $"err:{e.Code}");

        matched.Should().Be("err:test.failed");
    }

    [Fact]
    public void Match_WhenDefault_ShouldInvokeOnFailureWithDefaultError()
    {
        var result = default(Result<int>);

        var matched = result.Match(onSuccess: _ => "ok", onFailure: e => e.Code);

        matched.Should().Be("default");
    }

    [Fact]
    public void Match_WhenOnSuccessIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Match(null!, e => 0);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onSuccess");
    }

    [Fact]
    public void Match_WhenOnFailureIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Match(v => 0, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onFailure");
    }
    #endregion

    #region Map
    [Fact]
    public void Map_WhenSuccess_ShouldTransformTheValue()
    {
        var result = Result<int>.Ok(5);

        var mapped = result.Map(v => v * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_WhenSuccess_ShouldChangeTheResultType()
    {
        var result = Result<int>.Ok(5);

        Result<string> mapped = result.Map(v => $"n:{v}");

        mapped.Value.Should().Be("n:5");
    }

    [Fact]
    public void Map_WhenFailure_ShouldPreserveTheOriginalError()
    {
        // THE defining property of the railway: Map transforms the success track and passes the failure
        // track through *with its specific Error intact* — it must NOT launder it into DefaultError.
        var result = Result<int>.Fail(SomeError);

        var mapped = result.Map(v => v * 2);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Map_WhenFailure_ShouldNotInvokeTheProjection()
    {
        var invoked = false;
        var result = Result<int>.Fail(SomeError);

        _ = result.Map(v => { invoked = true; return v * 2; });

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Map_WhenProjectionIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Map<int>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("f");
    }
    #endregion

    #region Bind
    // Fallible steps used to prove chaining stays flat, short-circuits, and preserves errors.
    static Result<int> Halve(int n) => n % 2 == 0 ? n / 2 : OtherError;
    static Result<string> Label(int n) => $"={n}";

    [Fact]
    public void Bind_WhenSuccess_ShouldApplyFunctionAndStayFlat()
    {
        var result = Result<int>.Ok(8);

        // The result is Result<int>, NOT Result<Result<int>> — Bind does not re-wrap.
        Result<int> bound = result.Bind(Halve);

        bound.Value.Should().Be(4);
    }

    [Fact]
    public void Bind_WhenSuccessButFunctionFails_ShouldCarryTheNewError()
    {
        var result = Result<int>.Ok(7); // odd -> Halve fails with OtherError

        var bound = result.Bind(Halve);

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().BeSameAs(OtherError);
    }

    [Fact]
    public void Bind_WhenFailure_ShouldPreserveTheOriginalErrorAndNotInvokeFunction()
    {
        var invoked = false;
        var result = Result<int>.Fail(SomeError);

        var bound = result.Bind(v => { invoked = true; return Halve(v); });

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().BeSameAs(SomeError); // the *original* error, not a new one
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Bind_WhenChained_ShouldThreadThroughAllSteps()
    {
        var result = Result<int>.Ok(8);

        // 8 -> 4 -> 2 -> "=2"; every step stays single-level.
        var bound = result.Bind(Halve).Bind(Halve).Bind(Label);

        bound.Value.Should().Be("=2");
    }

    [Fact]
    public void Bind_WhenChainShortCircuits_ShouldPropagateTheFirstError()
    {
        var result = Result<int>.Ok(8);

        // 8 -> 4 -> 2 -> 1, then Halve(1) fails (odd) -> Label never runs, and OtherError rides to the end.
        var bound = result.Bind(Halve).Bind(Halve).Bind(Halve).Bind(Halve).Bind(Label);

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().BeSameAs(OtherError);
    }

    [Fact]
    public void Bind_WhenFunctionIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Bind<int>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("f");
    }
    #endregion

    #region Ensure
    [Fact]
    public void Ensure_WhenSuccessAndPredicateTrue_ShouldReturnTheOriginalResult()
    {
        var result = Result<int>.Ok(6);

        var ensured = result.Ensure(v => v % 2 == 0, SomeError);

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(6);
    }

    [Fact]
    public void Ensure_WhenSuccessAndPredicateFalse_ShouldFailWithTheSuppliedError()
    {
        // Unlike Option.Filter (which demotes to a None carrying no information), Ensure must be *told*
        // which Error to fail with — a failure needs a reason.
        var result = Result<int>.Ok(7);

        var ensured = result.Ensure(v => v % 2 == 0, SomeError);

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Ensure_WhenAlreadyFailed_ShouldPreserveTheOriginalError()
    {
        var result = Result<int>.Fail(SomeError);

        // The guard's own error (OtherError) must NOT replace the error already on the rail.
        var ensured = result.Ensure(v => true, OtherError);

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Ensure_WhenAlreadyFailed_ShouldNotInvokeThePredicate()
    {
        var invoked = false;
        var result = Result<int>.Fail(SomeError);

        _ = result.Ensure(v => { invoked = true; return true; }, OtherError);

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Ensure_WhenPredicateIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Ensure(null!, SomeError);

        act.Should().Throw<ArgumentNullException>().WithParameterName("predicate");
    }

    [Fact]
    public void Ensure_WhenErrorIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Ensure(v => true, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }
    #endregion

    #region Tap
    [Fact]
    public void Tap_WhenSuccess_ShouldRunOnSuccessWithValueAndReturnSameResult()
    {
        var seen = 0;
        var result = Result<int>.Ok(5);

        var returned = result.Tap(v => seen = v);

        seen.Should().Be(5);
        returned.Should().Be(result);
    }

    [Fact]
    public void Tap_WhenFailure_ShouldNotRunOnSuccessAndReturnSameResult()
    {
        var ran = false;
        var result = Result<int>.Fail(SomeError);

        var returned = result.Tap(_ => ran = true);

        ran.Should().BeFalse();
        returned.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Tap_WhenSuccess_ShouldRunOnSuccessNotOnFailure()
    {
        var successRan = false;
        var failureRan = false;
        var result = Result<int>.Ok(5);

        result.Tap(onSuccess: _ => successRan = true, onFailure: _ => failureRan = true);

        successRan.Should().BeTrue();
        failureRan.Should().BeFalse();
    }

    [Fact]
    public void Tap_WhenFailure_ShouldRunOnFailureWithTheError()
    {
        Error? seen = null;
        var result = Result<int>.Fail(SomeError);

        // The failure arm receives the Error — this is what makes Tap useful for diagnostics
        // ("log *why* it failed"), and is the asymmetry with Option.Tap's parameterless none arm.
        result.Tap(onSuccess: _ => { }, onFailure: e => seen = e);

        seen.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Tap_WhenDefault_ShouldRunOnFailureWithDefaultError()
    {
        Error? seen = null;
        var result = default(Result<int>);

        result.Tap(onSuccess: _ => { }, onFailure: e => seen = e);

        seen.Should().BeSameAs(DefaultError.Instance);
    }

    [Fact]
    public void Tap_WhenChained_ShouldReturnSameResultAtEachStep()
    {
        var result = Result<int>.Ok(5);

        var returned = result.Tap(_ => { }).Tap(_ => { });

        returned.Should().Be(result);
    }

    [Fact]
    public void Tap_WhenOnSuccessIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Tap(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onSuccess");
    }

    [Fact]
    public void Tap_WhenOnFailureIsNull_ShouldThrow()
    {
        var result = Result<int>.Ok(1);

        var act = () => result.Tap(_ => { }, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onFailure");
    }

    #endregion

    #region GetValueOr

    [Fact]
    public void GetValueOr_WhenSuccess_ShouldReturnTheValue()
    {
        var result = Result<int>.Ok(5);

        result.GetValueOr(99).Should().Be(5);
    }

    [Fact]
    public void GetValueOr_WhenFailure_ShouldReturnTheFallback()
    {
        var result = Result<int>.Fail(SomeError);

        result.GetValueOr(99).Should().Be(99);
    }

    [Fact]
    public void GetValueOr_WhenDefault_ShouldReturnTheFallback()
    {
        var result = default(Result<int>);

        result.GetValueOr(99).Should().Be(99);
    }

    [Fact]
    public void GetValueOr_WhenFallbackIsNull_ShouldThrow()
    {
        var result = Result<string>.Fail(SomeError);

        var act = () => result.GetValueOr(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("fallback");
    }

    #endregion

    #region Pipelines — the railway end to end

    [Fact]
    public void Pipeline_WhenAllStepsSucceed_ShouldReachTheSuccessBranch()
    {
        var outcome = Result<int>.Ok(8)
                                 .Ensure(v => v > 0, SomeError)
                                 .Map(v => v * 2)
                                 .Bind(Halve)
                                 .Match(onSuccess: v => $"ok:{v}", onFailure: e => $"err:{e.Code}");

        outcome.Should().Be("ok:8");
    }

    [Fact]
    public void Pipeline_WhenAStepFails_ShouldShortCircuitToTheFailureBranchWithThatError()
    {
        var mapInvoked = false;

        var outcome = Result<int>.Ok(-1)
                                 .Ensure(v => v > 0, SomeError)          // fails here
                                 .Map(v => { mapInvoked = true; return v * 2; })
                                 .Match(onSuccess: v => $"ok:{v}", onFailure: e => $"err:{e.Code}");

        outcome.Should().Be("err:test.failed");
        mapInvoked.Should().BeFalse(); // everything downstream of the failure is skipped
    }

    #endregion
}
