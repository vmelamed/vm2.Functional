// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class ResultTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    static readonly TestError SomeError = new("test.failed", "the operation failed");
    static readonly TestError OtherError = new("test.other", "a different failure");

    #region Construction, Ok / Fail, implicit conversions
    [Fact]
    public void Ok_ShouldBeSuccessAndCarryTheValue()
    {
        var result = Ok<int>(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Fail_ShouldBeFailureAndCarryTheError()
    {
        var result = Fail<int>(SomeError);

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
        var result = Fail<int>(SomeError);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Error_WhenSuccess_ShouldThrow()
    {
        var result = Ok<int>(42);

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
        var result = Ok<int>(7);

        var matched = result.Match(onSuccess: v => $"ok:{v}", onFailure: e => $"err:{e.Code}");

        matched.Should().Be("ok:7");
    }

    [Fact]
    public void Match_WhenFailure_ShouldInvokeOnFailureWithTheError()
    {
        var result = Fail<int>(SomeError);

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
        var result = Ok<int>(1);

        var act = () => result.Match(null!, e => 0);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onSuccess");
    }

    [Fact]
    public void Match_WhenOnFailureIsNull_ShouldThrow()
    {
        var result = Ok<int>(1);

        var act = () => result.Match(v => 0, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onFailure");
    }
    #endregion

    #region Map
    [Fact]
    public void Map_WhenSuccess_ShouldTransformTheValue()
    {
        var result = Ok<int>(5);

        var mapped = result.Map(v => v * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_WhenSuccess_ShouldChangeTheResultType()
    {
        var result = Ok<int>(5);

        Result<string> mapped = result.Map(v => $"n:{v}");

        mapped.Value.Should().Be("n:5");
    }

    [Fact]
    public void Map_WhenFailure_ShouldPreserveTheOriginalError()
    {
        // THE defining property of the railway: Map transforms the success track and passes the failure
        // track through *with its specific Error intact* — it must NOT launder it into DefaultError.
        var result = Fail<int>(SomeError);

        var mapped = result.Map(v => v * 2);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Map_WhenFailure_ShouldNotInvokeTheProjection()
    {
        var invoked = false;
        var result = Fail<int>(SomeError);

        _ = result.Map(v => { invoked = true; return v * 2; });

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Map_WhenProjectionIsNull_ShouldThrow()
    {
        var result = Ok<int>(1);

        var act = () => result.Map<int>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }
    #endregion

    #region Bind
    // Fallible steps used to prove chaining stays flat, short-circuits, and preserves errors.
    static Result<int> Halve(int n) => n % 2 == 0 ? n / 2 : OtherError;
    static Result<string> Label(int n) => $"={n}";

    [Fact]
    public void Bind_WhenSuccess_ShouldApplyFunctionAndStayFlat()
    {
        var result = Ok<int>(8);

        // The result is Result<int>, NOT Result<Result<int>> — Bind does not re-wrap.
        Result<int> bound = result.Bind(Halve);

        bound.Value.Should().Be(4);
    }

    [Fact]
    public void Bind_WhenSuccessButFunctionFails_ShouldCarryTheNewError()
    {
        var result = Ok<int>(7); // odd -> Halve fails with OtherError

        var bound = result.Bind(Halve);

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().BeSameAs(OtherError);
    }

    [Fact]
    public void Bind_WhenFailure_ShouldPreserveTheOriginalErrorAndNotInvokeFunction()
    {
        var invoked = false;
        var result = Fail<int>(SomeError);

        var bound = result.Bind(v => { invoked = true; return Halve(v); });

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().BeSameAs(SomeError); // the *original* error, not a new one
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Bind_WhenChained_ShouldThreadThroughAllSteps()
    {
        var result = Ok<int>(8);

        // 8 -> 4 -> 2 -> "=2"; every step stays single-level.
        var bound = result.Bind(Halve).Bind(Halve).Bind(Label);

        bound.Value.Should().Be("=2");
    }

    [Fact]
    public void Bind_WhenChainShortCircuits_ShouldPropagateTheFirstError()
    {
        var result = Ok<int>(8);

        // 8 -> 4 -> 2 -> 1, then Halve(1) fails (odd) -> Label never runs, and OtherError rides to the end.
        var bound = result.Bind(Halve).Bind(Halve).Bind(Halve).Bind(Halve).Bind(Label);

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().BeSameAs(OtherError);
    }

    [Fact]
    public void Bind_WhenFunctionIsNull_ShouldThrow()
    {
        var result = Ok<int>(1);

        var act = () => result.Bind<int>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("selector");
    }
    #endregion

    #region Ensure
    [Fact]
    public void Ensure_WhenSuccessAndPredicateTrue_ShouldReturnTheOriginalResult()
    {
        var result = Ok<int>(6);

        var ensured = result.Ensure(v => v % 2 == 0, SomeError);

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(6);
    }

    [Fact]
    public void Ensure_WhenSuccessAndPredicateFalse_ShouldFailWithTheSuppliedError()
    {
        // Unlike Option.Filter (which demotes to a None carrying no information), Ensure must be *told*
        // which Error to fail with — a failure needs a reason.
        var result = Ok<int>(7);

        var ensured = result.Ensure(v => v % 2 == 0, SomeError);

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Ensure_WhenAlreadyFailed_ShouldPreserveTheOriginalError()
    {
        var result = Fail<int>(SomeError);

        // The guard's own error (OtherError) must NOT replace the error already on the rail.
        var ensured = result.Ensure(v => true, OtherError);

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Ensure_WhenAlreadyFailed_ShouldNotInvokeThePredicate()
    {
        var invoked = false;
        var result = Fail<int>(SomeError);

        _ = result.Ensure(v => { invoked = true; return true; }, OtherError);

        invoked.Should().BeFalse();
    }

    [Fact]
    public void Ensure_WhenPredicateIsNull_ShouldThrow()
    {
        var result = Ok<int>(1);

        var act = () => result.Ensure(null!, SomeError);

        act.Should().Throw<ArgumentNullException>().WithParameterName("predicate");
    }

    [Fact]
    public void Ensure_WhenErrorIsNull_ShouldThrow()
    {
        var result = Ok<int>(1);

        var act = () => result.Ensure(v => true, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }
    #endregion

    #region Tap
    [Fact]
    public void Tap_WhenSuccess_ShouldRunOnSuccessWithValueAndReturnSameResult()
    {
        var seen = 0;
        var result = Ok<int>(5);

        var returned = result.Tap(v => seen = v);

        seen.Should().Be(5);
        returned.Should().Be(result);
    }

    [Fact]
    public void Tap_WhenFailure_ShouldNotRunOnSuccessAndReturnSameResult()
    {
        var ran = false;
        var result = Fail<int>(SomeError);

        var returned = result.Tap(_ => ran = true);

        ran.Should().BeFalse();
        returned.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Tap_WhenSuccess_ShouldRunOnSuccessNotOnFailure()
    {
        var successRan = false;
        var failureRan = false;
        var result = Ok<int>(5);

        result.Tap(onSuccess: _ => successRan = true, onFailure: _ => failureRan = true);

        successRan.Should().BeTrue();
        failureRan.Should().BeFalse();
    }

    [Fact]
    public void Tap_WhenFailure_ShouldRunOnFailureWithTheError()
    {
        Error? seen = null;
        var result = Fail<int>(SomeError);

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
        var result = Ok<int>(5);

        var returned = result.Tap(_ => { }).Tap(_ => { });

        returned.Should().Be(result);
    }

    [Fact]
    public void Tap_WhenOnSuccessIsNull_ShouldThrow()
    {
        var result = Ok<int>(1);

        var act = () => result.Tap(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onSuccess");
    }

    [Fact]
    public void Tap_WhenOnFailureIsNull_ShouldThrow()
    {
        var result = Ok<int>(1);

        var act = () => result.Tap(_ => { }, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("onFailure");
    }
    #endregion

    #region GetValueOr
    [Fact]
    public void GetValueOr_WhenSuccess_ShouldReturnTheValue()
    {
        var result = Ok<int>(5);

        result.GetValueOr(99).Should().Be(5);
    }

    [Fact]
    public void GetValueOr_WhenFailure_ShouldReturnTheFallback()
    {
        var result = Fail<int>(SomeError);

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
        var result = Fail<string>(SomeError);

        var act = () => result.GetValueOr(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("fallback");
    }
    #endregion

    #region Pipelines — the railway end to end
    [Fact]
    public void Pipeline_WhenAllStepsSucceed_ShouldReachTheSuccessBranch()
    {
        var outcome = Ok<int>(8)
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

        var outcome = Ok<int>(-1)
                                 .Ensure(v => v > 0, SomeError)          // fails here
                                 .Map(v => { mapInvoked = true; return v * 2; })
                                 .Match(onSuccess: v => $"ok:{v}", onFailure: e => $"err:{e.Code}");

        outcome.Should().Be("err:test.failed");
        mapInvoked.Should().BeFalse(); // everything downstream of the failure is skipped
    }
    #endregion

    #region Reference types

    // Every test above uses Result<int> — a value type, where the internal _value is a Nullable<int>.
    // With a reference T the storage and the null-forgiving unwraps take a structurally different path,
    // so the combinators are exercised again over references here.
    sealed record Customer(string Name);

    // NOTE: Result<Error> does not compile. Both implicit operators — Result<T>(T) and Result<T>(Error) —
    // apply when T is Error, and the compiler reports CS0457 "Ambiguous user defined conversions".
    // This is the safe failure mode (a compile error, not a silently wrong conversion), so Result<Error>
    // is simply an unsupported instantiation rather than a latent trap.

    [Fact]
    public void Ok_WhenReferenceValue_ShouldBeSuccessAndCarryTheReference()
    {
        var customer = new Customer("Ada");

        var result = Ok<Customer>(customer);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(customer);
    }

    [Fact]
    public void Ok_WhenReferenceValueIsNull_ShouldThrow()
    {
        // The ctor guard is only provable with a reference type — an int cannot be null.
        var act = () => Ok<Customer>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Fact]
    public void Fail_WhenReferenceType_ShouldCarryTheErrorAndThrowOnValue()
    {
        var result = Fail<Customer>(SomeError);

        result.Error.Should().BeSameAs(SomeError);

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Default_WhenReferenceType_ShouldBeFailureCarryingDefaultError()
    {
        // Here the defaulted _value is a null *reference*, not a Nullable<T> with HasValue == false.
        var result = default(Result<Customer>);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(DefaultError.Instance);
    }

    [Fact]
    public void Map_WhenReferenceToReference_ShouldTransformTheValue()
    {
        var result = Ok<Customer>(new Customer("Grace"));

        Result<string> mapped = result.Map(c => c.Name);

        mapped.Value.Should().Be("Grace");
    }

    [Fact]
    public void Map_WhenReferenceTypeFailure_ShouldPreserveTheOriginalError()
    {
        var result = Fail<Customer>(SomeError);

        var mapped = result.Map(c => c.Name);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Bind_WhenReferenceTypeChainShortCircuits_ShouldPropagateTheError()
    {
        static Result<Customer> RequireNamed(Customer c)
            => string.IsNullOrWhiteSpace(c.Name) ? OtherError : c;

        var result = Ok<Customer>(new Customer(""));   // blank name -> RequireNamed fails

        var bound = result.Bind(RequireNamed).Map(c => c.Name);

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().BeSameAs(OtherError);
    }

    [Fact]
    public void Ensure_WhenReferenceTypeAndPredicateFalse_ShouldFailWithTheSuppliedError()
    {
        var result = Ok<Customer>(new Customer(""));

        var ensured = result.Ensure(c => c.Name.Length > 0, SomeError);

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void Tap_WhenReferenceType_ShouldReceiveTheReferenceAndTheError()
    {
        Customer? seenValue = null;
        Error? seenError = null;

        Ok<Customer>(new Customer("Ada")).Tap(c => seenValue = c, e => seenError = e);
        Fail<Customer>(SomeError).Tap(c => seenValue = c, e => seenError = e);

        seenValue!.Name.Should().Be("Ada");
        seenError.Should().BeSameAs(SomeError);
    }

    [Fact]
    public void GetValueOr_WhenReferenceTypeFailure_ShouldReturnTheFallbackReference()
    {
        var fallback = new Customer("fallback");
        var result = Fail<Customer>(SomeError);

        result.GetValueOr(fallback).Should().BeSameAs(fallback);
    }

    #endregion
}
