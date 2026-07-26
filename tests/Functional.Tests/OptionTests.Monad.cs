// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class OptionTests
{
    #region F1. Functor Identity Law: m.Map(x => x) == m
    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ValueOption()
    {
        var option = Return(42);

        option.Map(x => x).Should().Be(option);
    }

    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ValueOption_None()
    {
        Option<int> option = None;

        option.Map(x => x).Should().Be(option);
    }

    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ReferenceOption()
    {
        Option<string> option = Return("forty two");

        option.Map(x => x).Should().Be(option);
    }

    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ReferenceOption_None()
    {
        Option<string> option = None;

        option.Map(x => x).Should().Be(option);
    }
    #endregion

    #region F2. Functor Composition Law: m.Map(f).Map(g) == m.Map(x => f(g(x)))
    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ValueOption()
    {
        Option<int> option = Return(42);

        Func<int, int> f = x => x + 1;
        Func<int, int> g = x => x * 2;

        option.Map(f).Map(g).Should().Be(option.Map(x => g(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ValueOption_None()
    {
        Option<int> option = None;

        Func<int, int> f = x => x + 1;
        Func<int, int> g = x => x * 2;

        option.Map(f).Map(g).Should().Be(option.Map(x => g(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ReferenceOption()
    {
        Option<string> option = Return("forty two");

        Func<string, string> f = x => x + "!";
        Func<string, string> g = x => x.ToUpper();

        option.Map(f).Map(g).Should().Be(option.Map(x => g(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ReferenceOption_None()
    {
        Option<string> option = None;

        Func<string, string> f = x => x + "!";
        Func<string, string> g = x => x.ToUpper();

        option.Map(f).Map(g).Should().Be(option.Map(x => g(f(x))));
    }
    #endregion

    #region M1. Monad Left Identity Law: return x >>= f == f x
    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ValueOption()
    {
        int x = 42;
        Func<int, Option<int>> f = y => Return(y + 1);

        Return(x).Bind(f).Should().Be(f(x));
    }

    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ValueOption_None()
    {
        Option<int> option = None;
        bool called = false;
        Func<int, Option<int>> f = y => { called = true; return Return(y + 1); };

        option.Bind(f).Should().Be(option);
        called.Should().BeFalse();
    }

    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ReferenceOption()
    {
        string x = "forty two";
        Func<string, Option<string>> f = y => Return(y.ToUpper());

        Return(x).Bind(f).Should().Be(f(x));
    }

    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ReferenceOption_None()
    {
        Option<string> option = None;
        bool called = false;
        Func<string, Option<string>> f = y => { called = true; return Return(y.ToUpper()); };

        option.Bind(f).Should().Be(option);
        called.Should().BeFalse();
    }
    #endregion

    #region M2. Monad Right Identity Law: m >>= return == m
    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ValueOption()
    {
        Option<int> option = Return(42);

        option.Bind(Return<int>).Should().Be(option);
    }

    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ValueOption_None()
    {
        Option<int> option = None;

        option.Bind(Return<int>).Should().Be(option);
    }

    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ReferenceOption()
    {
        Option<string> option = Return("forty two");

        option.Bind(Return<string>).Should().Be(option);
    }

    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ReferenceOption_None()
    {
        Option<string> option = None;

        option.Bind(Return<string>).Should().Be(option);
    }
    #endregion

    #region M3. Monad Associativity Law: (m >>= f) >>= g == m >>= (x => f(x) >>= g)
    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ValueOption()
    {
        Option<int> m = Return(42);
        Func<int, Option<string>> f = x => x.ToString();
        Func<string, Option<int>> g = x => x.Length * 2;

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }

    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ValueOption_None()
    {
        Option<int> m = None;
        Func<int, Option<string>> f = x => x.ToString();
        Func<string, Option<int>> g = x => x.Length * 2;

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }

    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ReferenceOption()
    {
        Option<string> m = Return("forty two");
        Func<string, Option<int>> f = x => x.Length * 2;
        Func<int, Option<string>> g = x => x.ToString();

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }

    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ReferenceOption_None()
    {
        Option<string> m = None;
        Func<string, Option<int>> f = x => x.Length * 2;
        Func<int, Option<string>> g = x => x.ToString();

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }
    #endregion

    #region MB. Monad Bridge Law: m >>= f == m >>= return(f)
    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ValueOption()
    {
        Option<int> m = Return(42);
        Func<int, string> f = x => x.ToString();

        m.Map(f).Should().Be(m.Bind(x => Return(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ValueOption_None()
    {
        Option<int> m = None;
        Func<int, string> f = x => x.ToString();

        m.Map(f).Should().Be(m.Bind(x => Return(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ReferenceOption()
    {
        Option<string> m = Return("forty two");
        Func<string, int> f = x => x.Length;

        m.Map(f).Should().Be(m.Bind(x => Return(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ReferenceOption_None()
    {
        Option<string> m = None;
        Func<string, int> f = x => x.Length;

        m.Map(f).Should().Be(m.Bind(x => Return(f(x))));
    }
    #endregion
}
