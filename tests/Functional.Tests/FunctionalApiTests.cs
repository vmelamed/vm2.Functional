// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Tests.Functional;

public class FunctionalTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    [Fact]
    public void Echo_returns_value_when_present()
    {
        var result = FunctionalApi.Echo("hi", "fallback");
        result.Should().Be("hi");
    }

    [Fact]
    public void Echo_returns_fallback_when_null()
    {
        var result = FunctionalApi.Echo(null, "fallback");
        result.Should().Be("fallback");
    }
}
