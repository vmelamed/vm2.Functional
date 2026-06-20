# vm2.Functional — Claude Context

@~/.claude/CLAUDE.md
@~/repos/vm2/CLAUDE.md
@.github/CONVENTIONS.md

## Package Identity

- Repo: <https://github.com/vmelamed/vm2.Functional>
- NuGet: <https://www.nuget.org/packages/vm2.Functional/>
- Status: *TODO* — in design / Unpublished / Published, stable
- Target: .NET 10.0+

## What This Package Does

*TODO* One-paragraph description of the package's purpose and the problem it solves.

Key design decisions:

- *TODO*

## Common Local Commands

```bash
# Build
dotnet build vm2.Functional.slnx

# Run tests (xUnit v3, MTP v2 — each project is a compiled executable)
dotnet test --project tests/Functional.Tests/Functional.Tests.csproj

# Run test executables (xUnit v3, MTP v2 — each project is a compiled to an executable) on Linux:
tests/Functional/bin/Debug/net10.0/Functional.Tests # or
tests/Functional/bin/Debug/net10.0/Functional.Tests.exe #  on Windows

# Run a single test by method name (xUnit v3, MTP v2 filter syntax)
dotnet test --project tests/Functional.Tests/Functional.Tests.csproj --filter "MethodName_WhenCondition_ShouldOutcome"

# Pack NuGet package
dotnet pack vm2.Functional.slnx --configuration Release

# Run benchmarks (Release only)
dotnet run --project benchmarks/Functional.Benchmarks --configuration Release -- --filter "*"

# If the benchmark tests are already built, you can run the compiled executable directly:
benchmarks/Functional.Benchmarks/bin/Release/net10.0/Functional.Benchmarks --help
benchmarks/Functional.Benchmarks/bin/Release/net10.0/Functional.Benchmarks --filter "*" # on Linux
benchmarks/Functional.Benchmarks/bin/Release/net10.0/Functional.Benchmarks.exe --filter "*" # on Windows
```

Tests use MTP v2 (Microsoft Testing Platform v2) with xUnit v3 — they compile to standalone executables.
Use `dotnet test --project <path>` per project; solution-wide `dotnet test` is not supported with MTP v2.

## Performance Characteristics

- *TODO* Hot paths, allocation behavior, benchmark numbers if known.

## Known Trade-offs and Design Notes

- *TODO*

## Active Work / Known Issues

- *TODO*

## Prompting Notes for This Package

- *TODO* Key invariants Claude must preserve, what to inject for testability, any non-obvious constraints.
