// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public class OptionTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    // A convenient "some" instance used across the equality tests.
    static Option<int> Some(int value) => value; // exercises the implicit T -> Option<T> conversion

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

    #region Equality — Option vs Option
    [Fact]
    public void Equals_WhenBothSomeWithEqualValues_ShouldBeTrue()
    {
        Some(1).Equals(Some(1)).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenBothSomeWithDifferentValues_ShouldBeFalse()
    {
        Some(1).Equals(Some(2)).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenBothNone_ShouldBeTrue()
    {
        // Regression guard: two "none" instances MUST be equal.
        Option<int> left = None;
        Option<int> right = None;

        left.Equals(right).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenOneSomeOneNone_ShouldBeFalse()
    {
        Option<int> some = 1;
        Option<int> none = None;

        some.Equals(none).Should().BeFalse();
        none.Equals(some).Should().BeFalse();
    }
    #endregion

    #region Equality — Option vs NoneType
    [Fact]
    public void EqualsNoneType_WhenNone_ShouldBeTrue()
    {
        Option<int> option = None;

        option.Equals(None).Should().BeTrue();
    }

    [Fact]
    public void EqualsNoneType_WhenSome_ShouldBeFalse()
    {
        Option<int> option = 1;

        option.Equals(None).Should().BeFalse();
    }
    #endregion

    #region Equality — Equals(object)
    [Fact]
    public void EqualsObject_WhenComparedToEqualSomeOption_ShouldBeTrue()
    {
        object other = Some(5);

        Some(5).Equals(other).Should().BeTrue();
    }

    [Fact]
    public void EqualsObject_WhenComparedToNoneType_ShouldReflectState()
    {
        object none = None;

        ((Option<int>)None).Equals(none).Should().BeTrue();
        Some(1).Equals(none).Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_WhenComparedToUnrelatedType_ShouldBeFalse()
    {
        Some(1).Equals("not an option").Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_WhenComparedToNull_ShouldBeFalse()
    {
        Some(1).Equals((object?)null).Should().BeFalse();
    }
    #endregion

    #region operator == / !=
    [Fact]
    public void OperatorEquals_WhenBothSomeEqual_ShouldBeTrue()
    {
        (Some(3) == Some(3)).Should().BeTrue();
        (Some(3) != Some(3)).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_WhenSomeVsNone_ShouldBeFalse()
    {
        Option<int> some = 3;
        Option<int> none = None;

        (some == none).Should().BeFalse();
        (some != none).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_WhenBothNone_ShouldBeTrue()
    {
        Option<int> left = None;
        Option<int> right = None;

        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
    }
    #endregion

    #region GetHashCode
    [Fact]
    public void GetHashCode_WhenEqualSome_ShouldMatch()
    {
        Some(9).GetHashCode().Should().Be(Some(9).GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenBothNone_ShouldMatch()
    {
        Option<int> left = None;
        Option<int> right = None;

        left.GetHashCode().Should().Be(right.GetHashCode());
    }
    #endregion

    #region Reference types
    // A value-equal reference type (the common domain case).
    sealed record Person(string Name, int Age);

    // A reference-equal type (default identity equality) to prove Option delegates to the type's own
    // equality via EqualityComparer<T>.Default rather than imposing value semantics of its own.
    sealed class Box(int value)
    {
        public int Value { get; } = value;
    }
    [Fact]
    public void ImplicitFromValue_WhenReferenceValueProvided_ShouldBeSome()
    {
        var person = new Person("Ada", 36);
        Option<Person> option = person;

        option.Equals(None).Should().BeFalse();
        option.Match(p => p.Name, () => "none").Should().Be("Ada");
    }

    [Fact]
    public void ImplicitFromValue_WhenRecordValueIsNull_ShouldThrow()
    {
        Person? nullPerson = null;

        var act = () => { Option<Person> _ = nullPerson!; };

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Fact]
    public void Equals_WhenSomeRecords_ShouldUseValueEquality()
    {
        // Distinct instances with equal content are equal because Person is a record.
        Option<Person> left = new Person("Grace", 45);
        Option<Person> right = new Person("Grace", 45);

        left.Equals(right).Should().BeTrue();
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_WhenSomeRecordsDiffer_ShouldBeFalse()
    {
        Option<Person> left = new Person("Grace", 45);
        Option<Person> right = new Person("Grace", 46);

        left.Equals(right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenSomeClassesWithReferenceEquality_ShouldFollowTypeSemantics()
    {
        var box = new Box(1);

        // Same reference -> equal; distinct references with equal content -> not equal,
        // because Box uses default (identity) equality. Option must not override that.
        Option<Box> sameRef1 = box;
        Option<Box> sameRef2 = box;
        Option<Box> otherRef = new Box(1);

        sameRef1.Equals(sameRef2).Should().BeTrue();
        sameRef1.Equals(otherRef).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenReferenceTypeNone_ShouldBeTrue()
    {
        Option<Person> left = None;
        Option<Person> right = None;

        left.Equals(right).Should().BeTrue();
        left.Equals(None).Should().BeTrue();
    }
    #endregion

    #region Reference types — combinators

    // The combinator tests above all use Option<int>. With a reference T the storage and the unwraps take a
    // structurally different path, so Map/Bind/Filter/Tap/GetValueOr are exercised again over references here.

    [Fact]
    public void Map_WhenSomeReference_ShouldProjectAndStaySome()
    {
        Option<Person> option = new Person("Ada", 36);

        Option<string> mapped = option.Map(p => p.Name);

        mapped.Match(s => s, () => "none").Should().Be("Ada");
    }

    [Fact]
    public void Map_WhenNoneReference_ShouldStayNoneAndNotInvokeProjection()
    {
        var invoked = false;
        Option<Person> option = None;

        var mapped = option.Map(p => { invoked = true; return p.Name; });

        mapped.Equals(None).Should().BeTrue();
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Bind_WhenReferenceChainShortCircuits_ShouldPropagateNone()
    {
        // A named person binds to their name; an unnamed one binds to None.
        static Option<string> NameOf(Person p) => string.IsNullOrWhiteSpace(p.Name) ? None : p.Name;

        Option<Person> named = new Person("Grace", 45);
        Option<Person> unnamed = new Person("", 0);

        named.Bind(NameOf).Match(s => s, () => "none").Should().Be("Grace");
        unnamed.Bind(NameOf).Equals(None).Should().BeTrue();
    }

    [Fact]
    public void Filter_WhenSomeReference_ShouldDemoteToNoneWhenPredicateFails()
    {
        Option<Person> option = new Person("Ada", 36);

        option.Filter(p => p.Age >= 18).Should().Be(option);          // passes -> unchanged
        option.Filter(p => p.Age >= 65).Equals(None).Should().BeTrue(); // fails  -> none
    }

    [Fact]
    public void Tap_WhenSomeReference_ShouldReceiveTheReference()
    {
        Person? seen = null;
        Option<Person> option = new Person("Ada", 36);

        var returned = option.Tap(p => seen = p);

        seen!.Name.Should().Be("Ada");
        returned.Should().Be(option);
    }

    [Fact]
    public void GetValueOr_WhenNoneReference_ShouldReturnTheFallbackReference()
    {
        var fallback = new Person("fallback", 0);
        Option<Person> option = None;

        option.GetValueOr(fallback).Should().BeSameAs(fallback);
    }

    #endregion
}
