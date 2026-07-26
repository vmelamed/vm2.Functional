// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class ResultTests
{
    #region Equals (typed and object)
    [Fact]
    public void Equals_WhenBothSuccessWithEqualValues_ShouldBeTrue()
    {
        Ok<int>(5).Equals(Ok<int>(5)).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenBothSuccessWithDifferentValues_ShouldBeFalse()
    {
        Ok<int>(5).Equals(Ok<int>(6)).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenBothFailureWithEqualErrors_ShouldBeTrue()
    {
        // Error is a record, so two failures carrying the same error are equal.
        Fail<int>(SomeError).Equals(Fail<int>(SomeError)).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenBothFailureWithDifferentErrors_ShouldBeFalse()
    {
        Fail<int>(SomeError).Equals(Fail<int>(OtherError)).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenSuccessVsFailure_ShouldBeFalse()
    {
        // The mismatched-state case: must return false, never throw (the earlier precedence bug).
        Ok<int>(5).Equals(Fail<int>(SomeError)).Should().BeFalse();
        Fail<int>(SomeError).Equals(Ok<int>(5)).Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenBothDefault_ShouldBeTrue()
    {
        default(Result<int>).Equals(default(Result<int>)).Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenDefaultVsExplicitDefaultError_ShouldBeTrue()
    {
        // A defaulted struct and an explicit Fail(DefaultError.Instance) both surface DefaultError,
        // so they compare equal. This is the deliberate normalization in Equals/GetHashCode.
        default(Result<int>).Equals(Fail<int>(DefaultError.Instance)).Should().BeTrue();
    }

    [Fact]
    public void EqualsObject_WhenComparedToEqualSuccess_ShouldBeTrue()
    {
        object other = Ok<int>(5);

        Ok<int>(5).Equals(other).Should().BeTrue();
    }

    [Fact]
    public void EqualsObject_WhenComparedToUnrelatedType_ShouldBeFalse()
    {
        Ok<int>(5).Equals("not a result").Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_WhenComparedToNull_ShouldBeFalse()
    {
        Ok<int>(5).Equals((object?)null).Should().BeFalse();
    }
    #endregion

    #region operator == / !=
    [Fact]
    public void OperatorEquals_WhenBothSuccessEqual_ShouldBeTrue()
    {
        (Ok<int>(3) == Ok<int>(3)).Should().BeTrue();
        (Ok<int>(3) != Ok<int>(3)).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_WhenSuccessVsFailure_ShouldBeFalse()
    {
        (Ok<int>(3) == Fail<int>(SomeError)).Should().BeFalse();
        (Ok<int>(3) != Fail<int>(SomeError)).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_WhenBothFailureEqual_ShouldBeTrue()
    {
        (Fail<int>(SomeError) == Fail<int>(SomeError)).Should().BeTrue();
        (Fail<int>(SomeError) != Fail<int>(SomeError)).Should().BeFalse();
    }
    #endregion

    #region GetHashCode
    [Fact]
    public void GetHashCode_WhenEqualSuccess_ShouldMatch()
    {
        Ok<int>(9).GetHashCode().Should().Be(Ok<int>(9).GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenEqualFailure_ShouldMatch()
    {
        Fail<int>(SomeError).GetHashCode().Should().Be(Fail<int>(SomeError).GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenBothDefault_ShouldMatch()
    {
        default(Result<int>).GetHashCode().Should().Be(default(Result<int>).GetHashCode());
    }

    [Fact]
    public void GetHashCode_WhenDefaultVsExplicitDefaultError_ShouldMatch()
    {
        // Consistency with Equals: since default equals Fail(DefaultError), their hashes must match too.
        default(Result<int>).GetHashCode().Should().Be(Fail<int>(DefaultError.Instance).GetHashCode());
    }
    #endregion

    #region .NET object identity rules
#pragma warning disable CS1718 // Comparison made to same variable
    [Fact]
    public void DotNet_ResultWithValue_Reflexivity()
    {
        Result<int> n1 = Ok(42);
        object n2 = n1;

        n1.Equals(n1).Should().BeTrue();
        n2.Equals(n2).Should().BeTrue();
        n1.Equals(n2).Should().BeTrue();
        (n1==n1).Should().BeTrue();
        (n2==n2).Should().BeTrue();

        n1 = Fail<int>(SomeError);
        n2 = n1;

        n1.Equals(n1).Should().BeTrue();
        n2.Equals(n2).Should().BeTrue();
        n1.Equals(n2).Should().BeTrue();
        (n1==n1).Should().BeTrue();
        (n2==n2).Should().BeTrue();
    }

    [Fact]
    public void DotNet_ResultWithReference_Reflexivity()
    {
        Result<Person> p1 = Ok(new Person("Ada", 36));
        object p2 = p1;

        p1.Equals(p1).Should().BeTrue();
        p2.Equals(p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();
        (p1==p1).Should().BeTrue();
        (p2==p2).Should().BeTrue();

        p1 = Fail<Person>(SomeError);
        p2 = p1;

        p1.Equals(p1).Should().BeTrue();
        p2.Equals(p2).Should().BeTrue();
        p1.Equals(p2).Should().BeTrue();
        (p1==p1).Should().BeTrue();
        (p2==p2).Should().BeTrue();
    }
#pragma warning restore CS1718 // Comparison made to same variable

    [Fact]
    public void DotNet_ResultWithValue_Symmetry()
    {
        Result<int> p1 = Ok(36);
        Result<int> p2 = Ok(36);

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue();

        p2 = Fail<int>(SomeError);

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide

        p2 = Ok(42);

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide

        p1 = Fail<int>(SomeError);
        p2 = p1;

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue();

        p1 = Fail<int>(SomeError);
        p2 = Fail<int>(OtherError);

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide
    }

    [Fact]
    public void DotNet_ResultWithReference_Symmetry()
    {
        Result<Person> p1 = Ok(new Person("Ada", 36));
        Result<Person> p2 = Ok(new Person("Ada", 36));

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue();

        p2 = Fail<Person>(SomeError);

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide

        p2 = Ok(new Person("Grace", 42));

        p1.Equals(p2).Should().BeFalse();

        (p1.Equals(p2) == p2.Equals(p1)).Should().BeTrue();
        ((p1==p2) == (p2==p1)).Should().BeTrue();
        // ((p1==p2) == (p1.GetHashCode() == p2.GetHashCode())).Should().BeTrue(); not necessarily true - the hash codes may collide
    }

    [Fact]
    public void DotNet_ResultWithValue_Transitivity()
    {
        Result<int> p1 = Ok(36);
        Result<int> p2 = Ok(36);
        Result<int> p3 = Ok(36);
        Result<int> p4 = Ok(42);

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

        p1 = Fail<int>(SomeError);
        p2 = Fail<int>(SomeError);
        p3 = Fail<int>(SomeError);
        p4 = Fail<int>(OtherError);

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


    [Fact]
    public void DotNet_ResultWithReference_Transitivity()
    {
        Result<Person> p1 = Ok(new Person("Ada", 36));
        Result<Person> p2 = Ok(new Person("Ada", 36));
        Result<Person> p3 = Ok(new Person("Ada", 36));
        Result<Person> p4 = Ok(new Person("Grace", 42));

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

        p1 = Fail<Person>(SomeError);
        p2 = Fail<Person>(SomeError);
        p3 = Fail<Person>(SomeError);
        p4 = Fail<Person>(OtherError);

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
