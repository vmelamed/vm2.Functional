// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public class FuncExtensionsTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    public static readonly TheoryData<string, bool> ToFunc_Data = new()
    {
        { TestLine(), false },
        { TestLine(), true },
    };

    [Theory]
    [MemberData(nameof(ToFunc_Data))]
    [Trait("FuncExtensions", "ToFunc")]
    public void ToFunc_ShouldConvertActionToFunc(
        string _,
        bool aNull)
    {
        var action = aNull ? null! : Substitute.For<Action<int>>();
        var act = () => action.ToFunc();

        if (aNull)
        {
            act.Should().Throw<ArgumentNullException>().WithParameterName("action");
            return;
        }

        var func = act.Should().NotThrow().Which;

        func(42).Should().Be(Unit.Instance);

        action.Received(1).Invoke(42);
    }

    public static readonly TheoryData<string, bool, bool, string> Compose_Data = new()
    {
        { TestLine(), false, false, "20" },
        { TestLine(), true,  false, ""   },
        { TestLine(), false, true,  ""   },
        { TestLine(), true,  true,  ""   },
    };

    [Theory]
    [MemberData(nameof(Compose_Data))]
    [Trait("FuncExtensions", "Compose")]
    public void Compose_ShouldThrowIfNullFunction(
        string _,
        bool f1Null,
        bool f2Null,
        string expected)
    {
        Func<int, int> f1    = f1Null ? null! : x => x * 2;
        Func<int, string> f2 = f2Null ? null! : x => x.ToString();

        var act = () => f1.Compose(f2);

        if (f1Null)
        {
            act.Should().Throw<ArgumentNullException>().WithParameterName("f1");
            return;
        }
        if (f2Null)
        {
            act.Should().Throw<ArgumentNullException>().WithParameterName("f2");
            return;
        }

        var composed = act.Should().NotThrow().Which;
        composed(10).Should().Be(expected);
    }

    [Fact]
    [Trait("FuncExtensions", "Apply")]
    public void Apply_ShouldInvokeFunction()
    {
        const int a1 = 1;
        const int a2 = 2;
        const int a3 = 3;
        const int a4 = 4;

        var func2 = Substitute.For<Func<int, int, int>>();
        func2.Invoke(a1, a2).Returns(22);
        var f2 = func2.Apply(a1);
        f2(a2).Should().Be(22);
        func2.Received(1).Invoke(a1, a2);

        var func3 = Substitute.For<Func<int, int, int, int>>();
        func3.Invoke(1, 2, 3).Returns(33);
        var f3 = func3.Apply(a1).Apply(a2);
        f3(a3).Should().Be(33);
        func3.Apply(a1).Apply(a2)(a3).Should().Be(33);
        func3.Received(1).Invoke(a1, a2, a3);

        var func4 = Substitute.For<Func<int, int, int, int, int>>();
        func4.Invoke(1, 2, 3, 4).Returns(44);
        var f4 = func4.Apply(a1).Apply(a2).Apply(a3);
        f4(a4).Should().Be(44);
        func4.Received(1).Invoke(a1, a2, a3, a4);
    }

    [Fact]
    [Trait("FuncExtensions", "Apply")]
    public void Apply_NullFunction_ShouldThrow()
    {
        Func<int, int, int>? func2 = null;
        var act2 = () => func2!.Apply(1);
        act2.Should().Throw<ArgumentNullException>().WithParameterName("f");

        Func<int, int, int, int>? func3 = null;
        var act3 = () => func3!.Apply(1);
        act3.Should().Throw<ArgumentNullException>().WithParameterName("f");

        Func<int, int, int, int, int>? func4 = null;
        var act4 = () => func4!.Apply(1);
        act4.Should().Throw<ArgumentNullException>().WithParameterName("f");
    }


    [Fact]
    [Trait("FuncExtensions", "Curry")]
    public void Curry_ShouldInvokeFunction()
    {
        const int a1 = 1;
        const int a2 = 2;
        const int a3 = 3;
        const int a4 = 4;

        var func2 = Substitute.For<Func<int, int, int>>();
        func2.Invoke(a1, a2).Returns(22);
        var f2 = func2.Curry();
        f2(a1)(a2).Should().Be(22);
        func2.Received(1).Invoke(a1, a2);

        var func3 = Substitute.For<Func<int, int, int, int>>();
        func3.Invoke(a1, a2, a3).Returns(33);
        var f3 = func3.Curry();
        f3(a1)(a2)(a3).Should().Be(33);
        func3.Received(1).Invoke(a1, a2, a3);

        var func4 = Substitute.For<Func<int, int, int, int, int>>();
        func4.Invoke(a1, a2, a3, a4).Returns(44);
        var f4 = func4.Curry();
        f4(a1)(a2)(a3)(a4).Should().Be(44);
        func4.Received(1).Invoke(a1, a2, a3, a4);
    }

    [Fact]
    [Trait("FuncExtensions", "Curry")]
    public void Curry_NullFunction_ShouldThrow()
    {
        Func<int, int, int>? func2 = null;
        var act2 = () => func2!.Curry();
        act2.Should().Throw<ArgumentNullException>().WithParameterName("f");

        Func<int, int, int, int>? func3 = null;
        var act3 = () => func3!.Curry();
        act3.Should().Throw<ArgumentNullException>().WithParameterName("f");

        Func<int, int, int, int, int>? func4 = null;
        var act4 = () => func4!.Curry();
        act4.Should().Throw<ArgumentNullException>().WithParameterName("f");
    }
}
