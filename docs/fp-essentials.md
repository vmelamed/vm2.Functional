# Functional Programming Essentials

A distilled reference for the FP concepts underpinning `vm2.Functional`. Each section states the idea, the takeaway, and a small snippet. It favors intuition over rigor; where a precise term exists (functor, monad), it is named but not belabored.

This document is descriptive, not normative — it explains the *why* behind the library's shapes. For the rules that govern how code is written, see `.github/CONVENTIONS.md`.

- [Functional Programming Essentials](#functional-programming-essentials)
  - [The two containers](#the-two-containers)
  - [Wrapping and lifting](#wrapping-and-lifting)
  - [Total vs. partial functions](#total-vs-partial-functions)
  - [Map — the functor operation](#map--the-functor-operation)
  - [Bind — the monad operation](#bind--the-monad-operation)
  - [Every monad is a functor](#every-monad-is-a-functor)
  - [The functor and monad laws](#the-functor-and-monad-laws)
    - [Functor laws (govern `Map`)](#functor-laws-govern-map)
    - [Monad laws (govern `Bind`)](#monad-laws-govern-bind)
    - [The bridge law (connects `Map` and `Bind`)](#the-bridge-law-connects-map-and-bind)
  - [Recovering from absence/failure — a separate, explicit operation](#recovering-from-absencefailure--a-separate-explicit-operation)
  - [Map vs. Bind, side by side](#map-vs-bind-side-by-side)
  - [Match — leaving the rails](#match--leaving-the-rails)
  - [Filter / Ensure — guarding the rail](#filter--ensure--guarding-the-rail)
  - [Tap — side effects without leaving](#tap--side-effects-without-leaving)
  - [Option vs. Result — the asymmetry](#option-vs-result--the-asymmetry)
  - [Exceptions vs. Result — the governing axis](#exceptions-vs-result--the-governing-axis)
  - [Railway-oriented programming](#railway-oriented-programming)
  - [Design principles that fell out](#design-principles-that-fell-out)
  - [Glossary of aliases](#glossary-of-aliases)

## The two containers

`vm2.Functional` provides two "containers" — types that wrap a value and add a context:

- **`Option<T>`** — *presence or absence*. Either `Some(value)` or `None`. Absence carries **no information**; all `None`s are identical. Replaces `null` as a domain value.
- **`Result<T>`** — *success or failure*. Either a success carrying a `T`, or a failure carrying an `Error`. Failure carries **information** (the `Error`: what went wrong, a stable `Code`, etc.).

Both are `where T : notnull` — null-as-a-value is not their job. If you need "a value that might legitimately be absent," that is `Option`'s `None`, not a nullable `T`.

## Wrapping and lifting

**Lifting** is moving something from the "plain" world into a "wrapped" world.

- Lift a **value**: a bare `T` becomes `Some(t)` / `Ok(t)`.
- Lift a **function**: `T -> R` becomes `Option<T> -> Option<R>` — this is what `Map` does.
- Lift a type you **don't own** into the wrapped world: e.g. `int? -> Option<int>` via `ToOption()`.

The unifying picture: there is a plain world (bare `T`, ordinary functions) and a wrapped world (`Option<T>`, `Result<T>`, `Task<T>`, …). Lifting carries things *up* into the wrapped world, and the wrapper contributes its own behavior (short-circuit on `None`, propagate the `Error`, await the task).

> **Generalization changes the *elements*; lifting changes the *world*, and the world brings its own rules.** Adding vectors generalizes `+` over richer elements; `Option`/`Task`/`Result` *lift* a function into a context that can skip, fail, or sequence it.

C#'s own `Nullable<T>` lifting is the same idea, specialized: `int? + int?` is the compiler *lifting* plain `int` `+` over `Nullable`, propagating null. In LINQ expression trees this is flagged as `IsLifted`. `Option<T>.Map` is the general, hand-rolled version of that exact operation.

## Total vs. partial functions

This distinction is the key to everything below.

- A ***total*** function always produces a value for every input. `int -> int`, `x => x * 2`. It **cannot fail**.
- A ***partial*** function has no answer for some inputs, so it reports "no result" / "failure". Its  return type *includes* that possibility: `int -> Option<int>`, `int -> Result<int>`.

```csharp
int Double(int n) => n * 2;                               // total   — always an int
Option<int> Halve(int n) => n % 2 == 0 ? n / 2 : None;    // partial — no answer for odd n
```

**`Map` is for *total* functions. `Bind` is for *partial* functions.** That single sentence is the whole difference between the two most important combinators.

## Map — the functor operation

`Map` applies a **total** function *inside* the container, without leaving it.

- Signature: `Option<T>.Map<R>(Func<T, R> f) -> Option<R>`.
- Success/some case: apply `f`, re-wrap the result.
- Failure/none case: pass through untouched (`None` stays `None`; a failure keeps its `Error`).
- The function **cannot fail** — `R` is a plain value — so `Map` never introduces a new `None`/failure.

```csharp
Some(5).Map(x => x * 2);          // Some(10)
Option<int> none = None;
none.Map(x => x * 2);             // None  — f never runs
```

A type with a lawful `Map` is a ***functor***.

## Bind — the monad operation

`Bind` chains a **partial** function — one that returns *its own* wrapped result — and keeps the result **flat**.

- Signature: `Option<T>.Bind<R>(Func<T, Option<R>> f) -> Option<R>`.
- The function returns `Option<R>` (it may itself produce `None`), and `Bind` returns that result **as-is** — it does **not** re-wrap. That is what keeps chains flat (no `Option<Option<R>>`).
- The first `None`/failure **short-circuits** the rest of the chain.

```csharp
Some(49).Bind(DivBy7)   // 49 / 7 = 7  -> Some(7)
        .Bind(DivBy7)   // 7  / 7 = 1  -> Some(1)
        .Bind(DivBy7)   // 1 not ÷ 7   -> None; anything after is skipped
        .Bind(DivBy7)   // none — short-circuited
        .Bind(DivBy7)   // none — short-circuited
        ;
```

Why not just use `Map`? Because `Map(partialFunction)` wraps the already-wrapped result and gives you `Option<Option<R>>` — nesting that deepens with every step and cannot be chained. `Bind` flattens; `Map` cannot. That flattening is the irreducible thing `Bind` adds.

A type with a lawful `Bind` is a **monad**. `Option`, `Result`, `Task`, and `IEnumerable` are all
monads — they differ only in what their "context" carries (absence, an error, asynchrony, multiplicity).

## Every monad is a functor

The two abstractions form a hierarchy, each layer being the one below it plus more power:

```text
Functor       (has Map)
   ⊂
Applicative   (has Map + Apply + Return)
   ⊂
Monad         (has Map + Apply + Bind + Return)
```

`Return` (also called `Pure`) is "lift a plain value into the container" — it is our `Some` / `Ok` (and the implicit converters): `T -> C<T>`. `Bind` is strictly more powerful than `Map`: **given `Bind` and `Return`, you can define `Map`**, but not the other way around (`Map` alone cannot flatten).

The identity: compose `f : T -> R` with `Return` to get `T -> C<R>` — exactly `Bind`'s parameter shape —
then feed it to `Bind`:

```csharp
// Map expressed via Bind + Return (Some is Return for Option):
Option<R> MapViaBind<R>(Func<T, R> f) where R : notnull => Bind(x => Some(f(x)));
// for Result, Return is Ok:                            => Bind(x => Ok(f(x)));
```

So every monad automatically satisfies the functor interface — a monad *is* an applicative *is* a
functor.

> This identity is a **law the code must satisfy**, not an implementation strategy. The library implements `Map` **directly** on the container's state (not via `Bind`) to avoid an extra delegate allocation and to keep the definition self-evident — but a direct `Map` must produce the *same* result as `Bind(x => Return(f(x)))` for all inputs. That equivalence, together with the monad laws (left identity, right identity, associativity), is exactly what a monad-law property test should verify.

For the formal proof that this derived `Map` obeys the functor laws (which in turn requires the monad laws), see Philip Wadler, *Monads for functional programming*, and Bartosz Milewski, *Category Theory for Programmers*.

## The functor and monad laws

`Map` being a functor and `Bind` being a monad are not free — they hold only if the operations obey a small set of **laws**. The laws are not decoration: each one guarantees a concrete refactoring is safe, and each one has a failure mode it exists to catch. A type that has the right method *shapes* but breaks a law is a lie — pipelines over it behave unpredictably. These are exactly the properties a `vm2.Functional` monadic type must have **executable tests** for (see CONVENTIONS, *Testing*).

In the formulas below, `m` is a container value (a `Some`/`Ok` **or** a `None`/failure — every law must hold for **both**), `Return` is `Some`/`Ok` (lift a value in), and `f`, `g` are functions.

### Functor laws (govern `Map`)

| #  | law             | formula                                         | guarantees / catches                                |
| :- | :-------------- | :---------------------------------------------- | :-------------------------------------------------- |
| F1 | **identity**    | `m.Map(x => x) == m`                            | Mapping "do nothing" does nothing — `Map` never fabricates, drops, or reorders. Forbids `None.Map(id)` from conjuring a value.                                                 |
| F2 | **composition** | `m.Map(f).Map(g) == m.Map(x => g(f(x)))`        | Two maps fuse into one — `Map` injects no behavior between steps; refactoring `.Map(f).Map(g)` into `.Map(g ∘ f)` is safe.                                                        |

### Monad laws (govern `Bind`)

| #  | law                | formula                                          | guarantees / catches                            |
| :- | :----------------- | :----------------------------------------------- | :---------------------------------------------- |
| M1 | **left identity**  | `Return(a).Bind(f) == f(a)`                      | `Return` adds no effect — wrapping then immediately binding is just calling `f`.                                                                                       |
| M2 | **right identity** | `m.Bind(Return) == m`                            | `Bind` preserves value and structure — binding to `Return` is a no-op round-trip; `None`/failure stays intact.                                                                |
| M3 | **associativity**  | `m.Bind(f).Bind(g) == m.Bind(x => f(x).Bind(g))` | Grouping does not matter — long pipelines can be freely re-parenthesized and sub-pipelines extracted into helpers.                                                              |

### The bridge law (connects `Map` and `Bind`)

| #  | law                      | formula                                 | guarantees / catches                               |
| :- | :------------------------| :-------------------------------------- | :------------------------------------------------- |
| MB | **Map/Bind consistency** | `m.Map(f) == m.Bind(x => Return(f(x)))` | The hand-written direct `Map` produces exactly what a `Bind`-derived `Map` would. **This is the highest-value test** — it is what a `Map` that dropped the error on the failure track (returning a generic default instead of propagating) would violate.                                                      |

**Three rules for writing the tests:**

1. **Assert every law on *both* states** — a `Some`/`Ok` **and** a `None`/failure. The failure-track cases (M2, MB on the failure branch) are where propagation bugs hide; the happy path is the easy half.
1. For `Result`, use a **test function that can actually fail** (e.g. `x => x % 2 == 0 ? Ok(x / 2) : Fail(err)`) so associativity and consistency exercise the error-propagation path, not just success.
1. The comparisons rely on the type's **value equality** (`Equals`/`==`) — so the law tests depend on the equality-contract tests being correct first.

```csharp
// M3, associativity — the trickiest, shown on both states:
Func<int, Option<int>> f = x => Some(x + 1);
Func<int, Option<int>> g = x => x % 2 == 0 ? Some(x / 2) : None;

Some(8).Bind(f).Bind(g).Should().Be(Some(8).Bind(x => f(x).Bind(g)));
((Option<int>)None).Bind(f).Bind(g).Should().Be(((Option<int>)None).Bind(x => f(x).Bind(g)));
```

## Recovering from absence/failure — a separate, explicit operation

`Map` and `Bind` are **structure-preserving**: their function runs *only* on the some/success track, and it can transform the *contents* of the container but never fabricate contents where there are none. A `None`/failure rides the other track **untouched**. This is not a limitation — it is required by the functor identity law (`m.Map(x => x) == m`): if `Map`/`Bind` could turn `None` into `Some(n)`, then `None.Map(x => x)` could yield `Some(n)`, contradicting the law. Mechanically, the reason is simpler still: on a `None`, the function is **never invoked** (`_isSome ? f(_value) : default`), so it has no
opportunity to conjure a value.

So **recovering from `None`/failure is a *different* operation** — one where *you* supply the replacement, rather than a transforming function inventing it:

| operation               | leaves the container?      | argument                                       | meaning              |
| :---------------------- | :------------------------- | :--------------------------------------------- | :------------------- |
| `GetValueOr(fallback)`  | **yes** — returns bare `T` | a **value** (`T`)                              | "exit with this guaranteed value if empty/failed"(terminal)                                                                                    |
| `OrElse(alternative)`   | no — returns `Option<T>`   | **another `Option<T>`**                        | "if empty, try this next source (which may also be empty)" (chainable)                                                                             |
| `Recover(f)` *(Result)* | no — returns `Result<T>`   | `Func<Error, T>` (or `Func<Error, Result<T>>`) | "if failed, compute a value/result *from the error*"                                                                                                 |

The distinction between `GetValueOr` and `OrElse`: `GetValueOr` provides a **guaranteed value** and **exits** (bare `T`, cannot chain); `OrElse` provides **another attempt that might itself fail** (stays `Option<T>`, chainable — "cache, else database, else remote"). `Recover` is `Result`'s version, and — per the usual asymmetry — its function **receives the `Error`**, because a `Result` failure carries the information a recovery may need; `Option`'s `OrElse` receives nothing, because absence carries none.

```csharp
int x         = none.GetValueOr(5);                    // 5      — bare int, done
Option<int> y = none.OrElse(cache).OrElse(remote);     // first Some, or None if all empty
Result<int> z = failed.Recover(err => Fallback(err));  // recovery computed from the error
```

None of these violate the structure-preserving rule, because you are not asking a *transforming*
function to invent a value — you are explicitly handing the operation the replacement (or a way to
derive it). (`OrElse` is the **alternative** combinator; together with `Apply` it is what a parser needs
for grammars with choices — "try this rule, else that one".)

## Map vs. Bind, side by side

|        | function shape   | can the function fail?       | result                   | chains…                     |
| :----- | :--------------- | :--------------------------- | :----------------------- | :-------------------------- |
| `Map`  | `T -> R`         | no — `R` is a plain value    | `Option<R>` (wraps once) | total operations            |
| `Bind` | `T -> Option<R>` | yes — it returns `Option<R>` | `Option<R>` (no re-wrap) | partial/fallible operations |

> - **`Map`'s function always returns a value.**
> - **`Bind`'s function returns**
>   - **a value *OR***
>   - **a verdict of failure/absence**
>
>   **It gets to say "stop here."**
>
> **The distinction is *can it fail?*, not *how often*.**

## Match — leaving the rails

`Match` is the one operation that **exits** the container, collapsing both cases to a single plain value.

- Signature: `Option<T>.Match<R>(Func<T, R> onSome, Func<R> onNone) -> R` (returns a **bare `R`**, not a container).
- It is the terminal step at a **boundary** where the outside world demands exactly one value of one type (an HTTP response, an exit code, a view model). The world is not a union, so the union must collapse — and `Match` is where you do it, explicitly, once.

```csharp
string label = Some(5).Match(onSome: v => $"value: {v}", onNone: () => "nothing");
```

Two disciplines:

1. **`Match` is the heavy exit — use it at the edge, roughly once per operation, not mid-pipeline.**
   Reaching for `Match` in the middle of logic is a smell; stay on the rails with `Map`/`Bind`/`Ensure`
   until the actual boundary.
2. **Collapse into a *rich* type, not a primitive.** `Result.Match(onSuccess, onError)` where `onError`
   *interprets* the error into the boundary's language (status + body, exit code, error banner) is
   `Match` used well. An `onError` that ignores its argument and returns a stock value is hand-washing —
   a misuse, not the concept's fault.

## Filter / Ensure — guarding the rail

Adding a predicate to the pipeline. Note the asymmetry between the two containers.

- **`Option.Filter(Func<T, bool> predicate)`** — if some and the predicate holds, unchanged; otherwise
  `None`. No extra argument, because a `None` carries no information.
- **`Result.Ensure(Func<T, bool> predicate, Error error)`** — if success and the predicate holds,
  unchanged; otherwise a failure carrying **that `error`**. It *must* be told which error to fail with,
  because a failure needs a reason. An already-failed `Result` keeps its original error and the
  predicate never runs.

```csharp
Some(6).Filter(n => n % 2 == 0);                       // Some(6)
Some(7).Filter(n => n % 2 == 0);                       // None
Ok(6).Ensure(n => n % 2 == 0, new OddError());         // Ok(6)
Ok(7).Ensure(n => n % 2 == 0, new OddError());         // Fail(OddError)
```

## Tap — side effects without leaving

`Tap` runs a side effect (logging, tracing) and returns the container **unchanged**, so it chains.

- `Option.Tap(Action<T> onSome)` / `Tap(Action<T> onSome, Action onNone)`.
- `Result.Tap(Action<T> onSuccess)` / `Tap(Action<T> onSuccess, Action<Error> onFailure)`.

Same asymmetry again: `Result`'s failure arm receives the **`Error`** (so it can log *why*); `Option`'s none arm receives **nothing** (absence has nothing to report).

```csharp
result.Tap(
    onSuccess: v => log.Info("ok: {Value}", v),
    onFailure: e => log.Error("failed: {Code}", e.Code));   // e is the Error
```

Side effects take `Action<T>`, not `Func<T, Unit>` — `Tap` discards the callback's result, so a `Unit` return would be ceremony (`return Unit.Instance;`) for no benefit. Extra inputs ride in via **closure capture**, never via arity overloads.

## Option vs. Result — the asymmetry

The two containers are *structurally* similar (both are two-case unions with the same combinator family) but *semantically* different. The difference shows up wherever the "failure" case is handled:

|                     | `Option<T>`                       | `Result<T>`                                 |
| :------------------ | :-------------------------------- | :------------------------------------------ |
| the two cases       | `Some(value)` / `None`            | `Ok<T>(value)` / `Fail<T>(Error)`           |
| failure carries     | **nothing** (absence has no data) | an **`Error`** (what/why)                   |
| `Match` failure arm | `Func<R>` — no argument           | `Func<Error, R>` — receives the error       |
| `Tap` failure arm   | `Action` — no argument            | `Action<Error>` — receives the error        |
| `Filter` / `Ensure` | `Filter(predicate)` — no error    | `Ensure(predicate, error)` — needs an error |

> **Symmetric *shape* is good (the same combinator family); symmetric *vocabulary* would be a lie.**
> Use `Some`/`None` and `Ok`/`Fail` — the universal FP names — precisely because the two types model
> different things. Do not force the verbs to match.

## Exceptions vs. Result — the governing axis

The library exists so that failure can be an ordinary, branch-able outcome rather than a thrown exception — but **not** everything should be a `Result`. The deciding axis is **contractual outcome vs. contract violation**, *not* recoverable vs. unrecoverable:

- **Exception** = the caller or environment is *broken*: a precondition was violated (a caller bug), or the machine failed. Guard clauses throw `ArgumentException`/`ArgumentNullException` — fail fast and loud.
- **`Result<T>`** = the operation ran correctly and *legitimately did not succeed* — a normal alternate outcome the caller is expected to branch on (not-found, a validation failure, a business-rule miss).

A failure may be unrecoverable and still be a `Result` (for signature visibility and error accumulation); a failure may be trivially recoverable and still be an exception, because it was a caller bug. See `.github/CONVENTIONS.md` (Error Handling) for the full rule, including the `Do`/`TryDo` dual pattern (bare-named throws at trusted call sites; `Try…` returns `Result` at boundaries).

## Railway-oriented programming

"Railway-oriented programming" (ROP) is the mental model for chaining `Result`-returning steps: think of two parallel tracks — a **success** track and a **failure** track.

- `Map` transforms the value *on the success track*; the failure track passes through untouched (**with its `Error` preserved** — never laundered into a generic error).
- `Bind` chains another fallible step; a failure anywhere **switches to the failure track** and every downstream step is skipped (short-circuit).
- `Ensure` is a switch that can *move* a value from success to failure if a predicate fails.
- `Match` is the only place the pipeline **leaves** the rails.

```csharp
Result<int>.Ok(8)
    .Ensure(v => v > 0, new NonPositive())   // stay on success track
    .Map(v => v * 2)                         // transform on success track
    .Bind(Halve)                             // fallible step; may switch tracks
    .Match(
        onSuccess: v => $"ok:{v}",           // leave the rails, once, at the edge
        onError:   e => $"err:{e.Code}");
```

The short-circuit lives **in the combinators**, written once. Business functions take the *unwrapped* value and return a `Result`; they never re-check upstream failure. `Match` is the single place a pipeline leaves the rails.

## Design principles that fell out

These recurred across the design and are worth stating on their own.

- **Make illegal states unrepresentable.** Discriminate success/some by an explicit `readonly` field, never by a public `init` property (a `with`-expression could desync it from the payload) and never by a field being null (collides with `default(struct)`). `default(Result<T>)` is a *benign failure* carrying `DefaultError`, not a landmine.
- **Predicates total, extractors partial.** `IsSuccess`/`IsFailure`/`IsSome` never throw. `Value`/`Error` throw when you ask for the wrong half — asking a failure for its value is a bug. `Option` has no throwing `.Value` at all: access is `Match`-only, so the none-case is unforgettable.
- **Compose, don't impersonate.** A monad is **not** a collection. `Option`/`Result` do not implement `IEnumerable<T>`. LINQ query syntax, if provided, comes from `Select`/`SelectMany`/`Where` *methods*, not from a false `is-a`. Query syntax binds to method names/shapes structurally — you get the capability without claiming an identity you do not have.
- **Instance methods for a type's own algebra.** Core combinators live *inside* the type, implemented directly on its private state — not via `Match`, not on top of each other (that inverts the dependency and allocates delegates). Extension methods are for adapting types you don't own (`T?.ToOption()`) and for the LINQ-name aliases.
- **Bridge vocabularies; keep the FP name primary.** `Map`/`Bind`/`Filter` are the primary (FP) names; `Select`/`SelectMany`/`Where` are thin aliases that exist so LINQ query syntax works.

## Glossary of aliases

The same operation appears under different names across FP traditions and .NET LINQ:

| this library    | LINQ (.NET)  | other FP names                                 |
| :-------------- | :----------- | :--------------------------------------------- |
| `Map`           | `Select`     | `fmap`, `<$>`, `Project`                       |
| `Bind`          | `SelectMany` | `flatMap`, `chain`, `then`, `>>=`              |
| `Filter`        | `Where`      | `filter`                                       |
| `Match`         | —            | `fold`, `cata`, `either`                       |
| `Some` / `None` | —            | `Just` / `Nothing` (Haskell)                   |
| `Ok` / `Fail`   | —            | `Ok` / `Err` (Rust), `Right` / `Left` (Either) |

Note: **"lift" is not a synonym for `Map`.** Lifting is the general act of moving a function into a
wrapped world; `Map` is one instance of it (the functor case). Do not list "Lift" as an alias of `Map`.
