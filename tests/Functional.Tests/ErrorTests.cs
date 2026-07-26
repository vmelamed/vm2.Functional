// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional.Tests;

public class ErrorTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    [Fact]
    public void AggregateError_ShouldContainAllErrors()
    {
        var error1 = new TestError("type1", "message1");
        var error2 = new TestError("type2", "message2");

        var aggregateError = new AggregateError(error1, error2);

        aggregateError.Code.Should().Be("aggregate");

        aggregateError.Message.Should().Be("message1\nmessage2");

        aggregateError
            .Errors
            .Should()
            .BeOfType<ImmutableList<Error>>().And
            .HaveCount(2).And
            .Contain(error1).And
            .Contain(error2);
    }
}
