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

Functional-programming primitives for C#: `Option<T>`, `Result<T>` / `Result`, `Error` (and its hierarchy),
`Unit`, and the combinators over them (`Match`, `Map`, `Bind`, `Filter`, `Tap`, …). The audience is **C#-first
.NET developers**, not Haskellers — the surface uses BCL-idiomatic naming and hides FP jargon unless it earns its
place. Buonanno's *Functional Programming in C#* (`LaYumba.Functional`, cloned at
`~/repos/functional-csharp-code-2`) is a **concept reference, not an implementation template** — its encodings
predate modern C# and its scope is maximalist; ours is deliberately thin.

Key design decisions:

- **`Option<T>` and `Result<T>` are hand-rolled `readonly struct`s, NOT `record struct`s.** This is a deliberate
  hedge for the coming C# **discriminated unions** — when DUs land, these types get rewritten to native `union`
  syntax, so we minimize entanglement with compiler-generated record plumbing. Do **not** recommend converting them
  to `record struct`; that debate is settled (see *Known Trade-offs*).
- **Success/some is discriminated by an explicit `readonly bool` field** (`_isSuccess` / `_isSome`), never by a
  public `init` property (which a `with`-expression could desync from the payload) and never by null-ness of a
  field (which collides with `default(struct)`). Predicates (`IsSuccess`/`IsFailure`) are **total** — they never
  throw; extractors (`Value`/`Error`) are **partial** — they throw when you ask for the wrong half.
- **`default(Result<T>)` is a benign failure carrying `DefaultError.Instance`**, not a throw. `default(Option<T>)`
  is `None`. Uninitialized structs are describable, not landmines.
- **`Option<T>` value access is `Match`-only** — no throwing `.Value` property. Forcing `Match`/`GetValueOr`/`ToOption`
  keeps the none-case unforgettable; a `.Value` would re-introduce the null-deref `Option` exists to kill.
- **`where T : notnull`** on both `Option<T>` and `Result<T>`. Null-as-value is not a `Result` concern — it is
  `Option`'s `None`. This constraint enforces the boundary rather than fighting it.
- **`Error` is an `abstract record`** with sealed concrete leaves; callers discriminate by **type**
  (`is EntityNotFoundError`, `is INotFoundError`), not by string-sniffing `Code`. `Error.Code` is a *stable external
  contract* for consumers who cannot see the CLR type (namespaced `<resource>.<kind>`), omitted for purely internal
  errors. Messages / help-links / i18n are **presentation concerns owned outward** (likely the TS/UI layer), never
  baked into the domain; a code catalogue, if ever built, is a reflection-generated manifest — deferred.
- **Combinators live *inside* the type**, implemented directly on `_isSome`/`_value` (or `_isSuccess`/`_error`) — not
  via `Match`, not on top of each other. Routing a core operation through the public surface inverts the dependency and
  allocates delegates (this is why `MapAlt`-via-`Match` was rejected). Extension methods are reserved for adapting
  types we don't own (`T?.ToOption()`, `Task<Result<T>>.BindAsync`) — see the instance-vs-extension rule in CONVENTIONS.
- **Side-effect combinators (`Tap`, `IfSome`/`IfNone`) take `Action<T>` / `Action`, not `Func<T, Unit>`.** Val
  weighed a `Func`-uniform internal calculus (keeping `Unit` everywhere, hiding it behind public adapters) against
  BCL-idiomatic `Action`, and **chose `Action`**. Rationale: `Tap` *discards* its callback's result (it returns
  `this` for chaining), so the `Unit` a `Func<T, Unit>` would produce is never composed on — you'd pay the
  `return Unit.Instance;` ceremony at every call site for a uniformity benefit `Tap` structurally cannot use.
  `Action<T>` also signals "returns nothing meaningful" more honestly than "returns the unit value I will ignore."
  `Unit` is still kept (see below) for places generics genuinely need it; it is simply not spread onto side-effect
  leaves. A now-deleted `Extensions.ToFunc(Action) -> Func<…,Unit>` helper (Buonanno-style, arities 0–16) was removed
  as YAGNI — it only earns its place under a `Func`-uniform design, which was not chosen. It may return if a real
  `Func<…,Unit>` call site ever appears.
- **`Tap` returns `this`, not `void`** (Buonanno's `ForEach` returns `void`) so side effects chain. The two-branch
  form is **two overloads**, not one method with a defaulted arm — `Tap(onSome)` and `Tap(onSome, onNone)` — so no
  `null` (which would be a foreign concept in a null-abolishing library) leaks into the signature. Each overload is a
  total function. Extra inputs to a side effect ride in via **closure capture**, never via arity overloads.
- **Compose, don't impersonate.** `Option`/`Result` are monads, **not** collections — they do not implement
  `IEnumerable<T>`. LINQ query syntax, if wanted, comes from `Select`/`SelectMany`/`Where` **methods**, never from a
  false `is-a`. (Buonanno bunches `Option` and `IEnumerable` into `C<T>` for *pedagogy*; that is a shared *abstraction*,
  not a mandate to share an *interface*.)
- **Construction verbs are `Some`/`None` and `Ok`/`Fail` — considered and kept over `Return`/`Pure`.** `Return`
  (FP's "lift a value into the monad", = our `Some`/`Ok`) was weighed and **rejected** for the public surface:
  (1) it **collides with the C# `return` keyword**, so `Return(5)` misreads as "returns 5" to a C#-first audience;
  (2) `Some`/`None` and `Ok`/`Fail` are **symmetric paired** state-names (each names its case) — `Return` breaks the
  pair, leaving no counterpart for `None`; (3) even in FP, `return` is a regretted name (Haskell moved to `pure`).
  `Return`/`Pure` are used only as *documentation aliases* (see `docs/fp-essentials.md`) and may appear as a
  test-local `Return = Some` helper in the monad-law tests — never as a member on the shipped type. This is the
  "keep the FP concept, don't multiply the surface" rule (cf. `Select`↔`Map`): the concept lives in the docs and the
  law-tests, not as a third construction path that would break the "one obvious way to construct" invariant.

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

- **Hand-rolled structs over `record struct` — the settled debate.** `record struct` would generate `Equals`/
  `GetHashCode`/`==`/`!=`/`ToString`/`with` for free and is *less code with identical semantics* for our equality needs.
  We chose the manual path anyway, for one reason: `Option`/`Result` are the exact types a future C# **discriminated-
  unions** feature will rewrite, so we keep them thin and independent of generated record machinery rather than having
  to un-inherit it during the migration. The cost we accept: hand-written equality is more error-prone (this bit us —
  `None == None` was briefly wrong, and `==`/`!=` briefly missed `static`), so **equality is covered by thorough unit
  tests** as the safety net. If you are tempted to "simplify" these to `record struct`, re-read this note first — it is
  a deliberate, argued choice, not an oversight.
- **`Match`-only `Option` may feel austere.** The escape hatch is `GetValueOr(fallback)` / `ToOption` at boundaries —
  add such extractors only when a real call site demands one, and keep them non-throwing (they still force the none-case
  to be named). Never add a throwing `.Value`.
- **`Unit` vs `void`.** We keep a `Unit` type (`Unit.Value` singleton) for the places generics cannot express `void`
  (e.g. `Result<Unit>` as the "succeeded, no payload" result, and `Func`-shaped composition points). We do **not**
  spread `Unit` onto side-effect leaves — see the `Tap`/`Action<T>` decision above. `Result` (non-generic helper) and
  `Result<Unit>` coexist: `Result.Ok()` is sugar over `Result<Unit>.Ok(default)`.
- **Coverage target: ~99%** for this package (it is small, foundational, and broadly depended on). The equality and
  combinator laws (functor/monad short-circuit, no double-wrap in `Bind`) are the highest-value tests.

## Active Work / Known Issues

- *TODO*

## Prompting Notes for This Package

- *TODO* Key invariants Claude must preserve, what to inject for testability, any non-obvious constraints.
