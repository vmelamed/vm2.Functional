// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public partial class ResultTests
{
    #region F1. Functor Identity Law: m.Map(x => x) == m
    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ValueResult_Right()
    {
        Result<int> result = Right(42);

        result.Map(x => x).Should().Be(result);
    }

    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ValueResult_Left()
    {
        Result<int> result = Left<int>(new TestError("F1", "Test error"));

        result.Map(x => x).Should().Be(result);
    }

    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ReferenceResult_Right()
    {
        Result<string> result = Right("forty two");

        result.Map(x => x).Should().Be(result);
    }

    [Fact]
    [Trait("MonadLaw", "F1. Functor Identity Law")]
    public void F1_FunctorIdentity_ReferenceResult_Left()
    {
        Result<string> result = Left<string>(new TestError("F1", "Test error"));

        result.Map(x => x).Should().Be(result);
    }
    #endregion

    #region F2. Functor Composition Law: m.Map(f).Map(g) == m.Map(x => f(g(x)))
    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ValueResult_Right()
    {
        Result<int> result = Right(42);

        Func<int, int> f = x => x + 1;
        Func<int, int> g = x => x * 2;

        result.Map(f).Map(g).Should().Be(result.Map(x => g(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ValueResult_Left()
    {
        Result<int> result = Left<int>(new TestError("F2", "Test error"));

        Func<int, int> f = x => x + 1;
        Func<int, int> g = x => x * 2;

        result.Map(f).Map(g).Should().Be(result.Map(x => g(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ReferenceResult_Right()
    {
        Result<string> result = Right("forty two");

        Func<string, string> f = x => x + "!";
        Func<string, string> g = x => x.ToUpper();

        result.Map(f).Map(g).Should().Be(result.Map(x => g(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "F2. Functor Composition Law")]
    public void F2_FunctorComposition_ReferenceResult_Left()
    {
        Result<string> result = Left<string>(new TestError("F2", "Test error"));

        Func<string, string> f = x => x + "!";
        Func<string, string> g = x => x.ToUpper();

        result.Map(f).Map(g).Should().Be(result.Map(x => g(f(x))));
    }
    #endregion

    #region M1. Monad Left Identity Law: right x >>= f == f x
    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ValueResult_Right()
    {
        int x = 42;
        Func<int, Result<int>> f = y => Right(y + 1);

        Right(x).Bind(f).Should().Be(f(x));
    }

    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ValueResult_Left()
    {
        Error error = new TestError("M1", "Test error");
        bool called = false;
        Func<int, Result<int>> f = y => { called = true; return Right(y + 1); };

        Left<int>(error).Bind(f).Should().Be(Left<int>(error));
        called.Should().BeFalse();
    }

    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ReferenceResult_Right()
    {
        string x = "forty two";
        Func<string, Result<string>> f = y => Right(y.ToUpper());

        Right(x).Bind(f).Should().Be(f(x));
    }

    [Fact]
    [Trait("MonadLaw", "M1. Monad Left Identity Law")]
    public void M1_MonadLeftIdentity_ReferenceResult_Left()
    {
        Error error = new TestError("M1", "Test error");
        bool called = false;
        Func<string, Result<string>> f = y => { called = true; return Right(y.ToUpper()); };

        Left<string>(error).Bind(f).Should().Be(Left<string>(error));
        called.Should().BeFalse();
    }
    #endregion

    #region M2. Monad Right Identity Law: m >>= return == m
    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ValueResult_Right()
    {
        Result<int> result = Right(42);

        result.Bind(Right<int>).Should().Be(result);
    }

    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ValueResult_Left()
    {
        Error error = new TestError("M2", "Test error");
        Result<int> result = Left<int>(error);

        result.Bind(Right<int>).Should().Be(result);
    }

    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ReferenceResult_Right()
    {
        Result<string> result = Right("forty two");

        result.Bind(Right<string>).Should().Be(result);
    }

    [Fact]
    [Trait("MonadLaw", "M2. Monad Right Identity Law")]
    public void M2_MonadRightIdentity_ReferenceResult_Left()
    {
        Error error = new TestError("M2", "Test error");
        Result<string> result = Left<string>(error);

        result.Bind(Right<string>).Should().Be(result);
    }
    #endregion

    #region M3. Monad Associativity Law: (m >>= f) >>= g == m >>= (x => f(x) >>= g)
    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ValueResult()
    {
        Result<int> m = Right(42);
        Func<int, Result<string>> f = x => x.ToString();
        Func<string, Result<int>> g = x => x.Length * 2;

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }

    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ValueResult_Left()
    {
        var error = new TestError("M3", "Test error");
        Result<int> m = Left<int>(error);
        Func<int, Result<string>> f = x => x.ToString();
        Func<string, Result<int>> g = x => x.Length * 2;

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }

    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ReferenceResult()
    {
        Result<string> m = Right("forty two");
        Func<string, Result<int>> f = x => x.Length * 2;
        Func<int, Result<string>> g = x => x.ToString();

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }

    [Fact]
    [Trait("MonadLaw", "M3. Monad Associativity Law")]
    public void M3_MonadAssociativity_ReferenceResult_Left()
    {
        var error = new TestError("M3", "Test error");
        Result<string> m = Left<string>(error);
        Func<string, Result<int>> f = x => x.Length * 2;
        Func<int, Result<string>> g = x => x.ToString();

        m.Bind(f).Bind(g).Should().Be(m.Bind(x => f(x).Bind(g)));
    }
    #endregion

    #region MB. Monad Bridge Law: m >>= f == m >>= return(f)
    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ValueResult()
    {
        Result<int> m = Right(42);
        Func<int, string> f = x => x.ToString();

        m.Map(f).Should().Be(m.Bind(x => Right(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ValueResult_None()
    {
        var error = new TestError("MB", "Test error");
        Result<int> m = Left<int>(error);
        Func<int, string> f = x => x.ToString();

        m.Map(f).Should().Be(m.Bind(x => Right(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ReferenceResult()
    {
        Result<string> m = Right("forty two");
        Func<string, int> f = x => x.Length;

        m.Map(f).Should().Be(m.Bind(x => Right(f(x))));
    }

    [Fact]
    [Trait("MonadLaw", "MB. Monad Bridge Law")]
    public void MB_MonadBridge_ReferenceResult_None()
    {
        var error = new TestError("MB", "Test error");
        Result<string> m = Left<string>(error);
        Func<string, int> f = x => x.Length;

        m.Map(f).Should().Be(m.Bind(x => Right(f(x))));
    }
    #endregion
}
