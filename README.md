# vm2.Functional

A starter vm2 package scaffold. Customize the code, tests, benchmarks, docs, and workflows as needed.

## Getting started

- Build:

  ```bash
  dotnet restore
  dotnet build
  ```

- Test:
  - from **CLI**, if it is not built yet (builds on MTP v2):

    ```bash
    dotnet run --project tests/Functional.Tests/Functional.Tests.csproj
    ```

  - from **CLI**, if it is already built in **CLI** or **VSCode** (MTP v2):
    - any OS or shell:

      ```bash
      dotnet test tests/Functional.Tests/bin/Debug/net10.0/Functional.Tests.dll
      ```

    - on Windows **CLI** (already built in **CLI** or **VSCode** - on MTP v2):

      ```batch
      tests/Functional.Tests/bin/Debug/net10.0/Functional.Tests.exe
      ```

    - on Linux or MacOS **CLI** (already built in **CLI** or **VSCode** - on MTP v2):

      ```bash
      tests/Functional.Tests/bin/Debug/net10.0/Functional.Tests
      ```

  - from Visual Studio:
    - use the Test Explorer to build and run tests (builds on MTP v1)
    - if it is already built in **Visual Studio** (MTP v1), from the **CLI** you can run:

      ```bash
      dotnet test
      ```

- Benchmarks (if included):

  ```bash
  dotnet run --project benchmarks/Functional.Benchmarks/Functional.Benchmarks.csproj --configuration Release
  ```

  > [!TIP]
  > In a personal development environment, you can run benchmarks with defined `SHORT_RUN` preprocessor directive. The
  run will be faster, although less accurate, but still suitable for quick iterations.

## Package metadata

- Package ID: `vm2.Functional`
- Version: 0.0.0
- License: MIT
- Repository: <https://github.com/vmelamed/vm2.Functional>

## Repository Layout

  ```text
  vm2.<name>/
  ├── .github/
  │   ├── dependabot.yml *      # dependabot configuration (see note below)
  │   ├── CONVENTIONS.md *      # Claude conventions for contributing to the repo
  │   ├── copilot-instructions.md
  │   ├── PULL_REQUEST_TEMPLATE.md *
  │   └── workflows/            # GitHub Actions workflows
  │       ├── AutoMerge.yaml *
  │       ├── ClearCache.yaml *
  │       ├── CI.yaml **
  │       ├── Prerelease.yaml **
  │       └── Release.yaml **
  ├── benchmarks/               # Benchmark projects (recommended)
  │   └── vm2.<name>.Benchmarks/
  │       ├── EchoBenchmarks.cs
  │       ├── <name>.Benchmarks.csproj
  │       ├── Program.cs
  │       └── usings.cs
  ├── changelog/                # git-cliff toml files for updating the Changelog from commit messages
  │   ├── cliff.prerelease.toml *
  │   └── cliff.release.toml *
  ├── docs/                     # Extra documentation - in addition to the README.md in the repo root (optional)
  │   └── README.md
  ├── examples/                 # Example program(s) (one file program(s) or project(s) - optional)
  │   └── Program.cs
  ├── src/                      # Source code
  │   └── <name>/
  │       ├── <name>.csproj
  │       ├── <name>.Api.cs
  │       └── usings.cs
  ├── tests/                    # Test projects (highly recommended)
  │   └── <name>.Tests/
  │       ├── <name>.Tests.csproj
  │       ├── <name>ApiTests.cs
  │       └── usings.cs
  ├── .editorconfig *
  ├── .gitattributes *
  ├── .gitmessage *
  ├── .gitignore *
  ├── CLAUDE.md
  ├── codecov.yaml *
  ├── coverage.settings.xml *
  ├── Directory.Build.props **
  ├── Directory.Packages.props **
  ├── global.json *
  ├── LICENSE *
  ├── NuGet.config *
  ├── README.md
  ├── testconfig.json *
  ├── vm2.Functional.slnx
  └── CHANGELOG.md
  ```

- .github/workflows: CI, prerelease, release, clear-cache.
- src/Functional: the library source code
- tests/Functional.Tests: xUnit + MTP tests, includes testconfig.json
- benchmarks/Functional.Benchmarks: BenchmarkDotNet suite (optional)
- examples/Functional.Example: minimal console sample (optional)
- docs/: documentation starter (optional)
- scripts/: bootstrap helpers
- changelog/: git-cliff configs for prerelease/release changelog updates

## Next steps

> [!TIP]
> Feel free to remove this section before release.

Create GitHub repository using the generated bootstrap script: `$VM2_REPOS/vm2.DevOps/scripts/bash/setup-repo.sh`. It will:
- Update README, CHANGELOG, and package metadata.
- Use the repository setup script `scripts/setup-repo.sh` to initialize the repository as follows:
  - create a local Git  repository and make the initial commit
  - set local Git configuration settings:
    - core.hooksPath                       = `$VM2_REPOS/vm2.DevOps/scripts/githooks`
    - commit.template                      = `.gitmessage`
    - merge.ff                             = `only`
    - **pull.rebase                          = `true`**
    - fetch.prune                          = `true`
    - push.autoSetupRemote                 = `true`
    - rerere.enabled                       = `true`
    - rerere.autoUpdate                    = `true`
    - rebase.autoStash                     = `true`
    - merge.conflictstyle                  = `zdiff3`
    - push.useForceIfIncludes              = `true`
    - tag.sort                             = `version:refname`
    - merge.nugetlock.name                 = `NuGet lockfile - take the incoming side and regenerate`
    - merge.nugetlock.driver               = `cp -f %B %A && echo "vm2: %P auto-resolved (took the incoming side) - regenerate with: dotnet restore --force-evaluate" >&2`
  - create a remote repository on GitHub and link it to the local repository
  - push the initial commit to the remote repository on GitHub
  - set repository settings:
    - **Default branch                       = `main`**
    - Has wiki                             = `false`
    - Has issues                           = `true`
    - Has projects                         = `false`
    - **Has pull requests                    = `true`**
    - Pull request creation policy         = `all`
    - Allow merge commit                   = `false`
    - Allow squash merge                   = `false`
    - **Allow rebase merge                   = `true`**
    - Allow auto merge                     = `true`
    - Delete branch on merge               = `true`
    - Visibility                           = `public`
    - Actions permissions:
      - Can approve pull request reviews   = true
      - Default workflow permissions       = read
  - protect the `main` branch by enabling required checks and requiring pull requests:
    - Enforcement                          = `active`
    - Repository admin bypass              = present
    - Deletion                             = present
    - Required linear history              = present
    - Pull request                         = present
    - Required approving review count      = present
    - Dismiss stale reviews on push        = present
    - Require code owner review            = present
    - Require last push approval           = present
    - Required review thread resolution    = present
    - Required reviewers                   = present
    - Allowed merge methods                = present
    - Required status checks               = present
    - Do not enforce on create             = present
    - Strict required status checks policy = present
    - Non fast forward                     = present
    - Required status checks list:
      - **Postrun-CI                           = present** (combines the results from build, test, benchmark, test package)
  - **interactively** set required variables for workflows:
    - CONFIGURATION                        = `Release`
    - DOTNET_VERSION                       = `10.0.x`
    - MAX_GEN1_COLLECTS                    = `2`
    - MAX_GEN2_COLLECTS                    = `1`
    - MAX_REGRESSION_PCT                   = `20`
    - MIN_COVERAGE_PCT                     = `80`
    - MINVERDEFAULTPRERELEASEIDENTIFIERS   = `preview.0`
    - MINVERTAGPREFIX                      = `v`
    - NUGET_SERVER                         = `github` (can be also `nuget`)
    - RESET_BENCHMARK_THRESHOLDS           = `false`
    - SAVE_PACKAGE_ARTIFACTS               = `false`
    - VERBOSE                              = `false` (use for workflow debugging)
    - ACTIONS_RUNNER_DEBUG                 = `false`
    - ACTIONS_STEP_DEBUG                   = `false`
  - **interactively** set required secrets for workflows (prepare the secrets in advance):
    - BENCH_DISPATCH_PAT                   = [secret]
    - BENCHER_API_TOKEN                    = [secret]
    - CODECOV_TOKEN                        = [secret]
    - NUGET_API_KEY                        = [secret]
    - RELEASE_PAT                          = [secret]
    - REPORTGENERATOR_LICENSE              = [secret]
    - Dependabot Secrets:
      - GH_PACKAGES_TOKEN                    = [secret]
- Changelog: prerelease workflow appends a prerelease section; release workflow adds a stable header with "See prereleases below." (prerelease sections stay intact).

> [!NOTE]
> The above settings and configurations may change over time and should be reviewed periodically to ensure they align with the desired workflow and security practices. The source of truth is the script `setup-repo.sh`. It is idempotent and can be run multiple times without causing unintended side effects. Also, can be run with option `--audit` to review the current settings and configurations against the defaults without making any changes.
