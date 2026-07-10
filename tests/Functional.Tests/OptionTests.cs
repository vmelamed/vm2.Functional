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
        string? nullString = null;

        // The private ctor's null guard fires through the implicit conversion.
        var act = () => { Option<string> _ = nullString!; };

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
        object none = NoneType.None;

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
    public void GetHashCode_WhenEqualSomes_ShouldMatch()
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
}
