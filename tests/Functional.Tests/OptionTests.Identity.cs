// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class OptionTests
{
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

    #region .NET object identity rules
#pragma warning disable CS1718 // Comparison made to same variable
    [Fact]
    public void DotNet_OptionWithValue_Reflexivity()
    {
        Option<int> n1 = Some(42);
        object n2 = n1;

        n1.Equals(n1).Should().BeTrue();
        n2.Equals(n2).Should().BeTrue();
        n1.Equals(n2).Should().BeTrue();
        (n1==n1).Should().BeTrue();
        (n2==n2).Should().BeTrue();

        n1 = None;
        n2 = n1;

        n1.Equals(n1).Should().BeTrue();
        n2.Equals(n2).Should().BeTrue();
        n1.Equals(n2).Should().BeTrue();
        (n1==n1).Should().BeTrue();
        (n2==n2).Should().BeTrue();
    }

    [Fact]
    public void DotNet_OptionWithReference_Reflexivity()
    {
        Option<Person> p1 = Some(new Person("Ada", 36));
        object p2 = p1;

        p1.Equals(p1).Should().BeTrue();
        p2.Equals(p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();
        (p1==p1).Should().BeTrue();
        (p2==p2).Should().BeTrue();

        p1 = None;
        p2 = p1;

        p1.Equals(p1).Should().BeTrue();
        p2.Equals(p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();
        (p1==p1).Should().BeTrue();
        (p2==p2).Should().BeTrue();
    }
#pragma warning restore CS1718 // Comparison made to same variable

    [Fact]
    public void DotNet_OptionWithValue_Symmetry()
    {
        Option<int> p1 = Some(36);
        Option<int> p2 = Some(36);

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue();

        p2 = None;

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide

        p2 = Some(42);

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide
    }

    [Fact]
    public void DotNet_OptionWithReference_Symmetry()
    {
        Option<Person> p1 = Some(new Person("Ada", 36));
        Option<Person> p2 = Some(new Person("Ada", 36));

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue();

        p2 = None;

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide

        p2 = Some(new Person("Grace", 42));

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide
    }

    [Fact]
    public void DotNet_OptionWithValue_Transitivity()
    {
        Option<int> p1 = Some(36);
        Option<int> p2 = Some(36);
        Option<int> p3 = Some(36);
        Option<int> p4 = Some(42);

        (p1.Equals(p2) && p2.Equals(p3)).Should().BeTrue(because: "The premise of transitivity must hold");
        p1.Equals(p3).Should().BeTrue();

        (p1 == p3).Should().BeTrue();
        (p3 == p1).Should().BeTrue();

        (p1 != p3).Should().BeFalse();
        (p3 != p1).Should().BeFalse();

        (p1.GetHashCode() == p3.GetHashCode()).Should().BeTrue();

        (p1.Equals(p2) && !p2.Equals(p4)).Should().BeTrue(because: "The premise of transitivity must hold");
        p1.Equals(p4).Should().BeFalse();

        (p1 == p4).Should().BeFalse();
        (p4 == p1).Should().BeFalse();

        (p1 != p4).Should().BeTrue();
        (p4 != p1).Should().BeTrue();

        // (p1.GetHashCode() != p4.GetHashCode()).Should().BeTrue(); not necessarily true - the hash codes may collide
    }

    [Fact]
    public void DotNet_OptionWithReference_Transitivity()
    {
        Option<Person> p1 = Some(new Person("Ada", 36));
        Option<Person> p2 = Some(new Person("Ada", 36));
        Option<Person> p3 = Some(new Person("Ada", 36));
        Option<Person> p4 = Some(new Person("Grace", 42));

        p1.Equals(p2).Should().BeTrue(because: "premise1 p1 == p2");
        p2.Equals(p3).Should().BeTrue(because: "premise1 p2 == p3");
        p1.Equals(p3).Should().BeTrue();

        (p1 == p3).Should().BeTrue();
        (p3 == p1).Should().BeTrue();

        (p1 != p3).Should().BeFalse();
        (p3 != p1).Should().BeFalse();

        (p1.GetHashCode() == p3.GetHashCode()).Should().BeTrue();

        p1.Equals(p2).Should().BeTrue(because: "premise1 p1 == p2");
        p2.Equals(p4).Should().BeFalse(because: "premise2 p2 != p4");
        p1.Equals(p4).Should().BeFalse();

        (p1 == p4).Should().BeFalse();
        (p4 == p1).Should().BeFalse();

        (p1 != p4).Should().BeTrue();
        (p4 != p1).Should().BeTrue();

        // (p1.GetHashCode() != p4.GetHashCode()).Should().BeTrue(); not necessarily true - the hash codes may collide
    }
    #endregion
}
