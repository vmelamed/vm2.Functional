# Changelog

## v1.0.0-preview.1 - 2026-10-27

### Added

- add Option<T> with core combinators and Some/None front door
- added Match Map Bind, Ensure, GetValueOr, Tap, and Tap2
- add comprehensive ResultTests for Result<T> functionality
- add combinator tests for Option<T> and Result<T> with reference types
- Add functional programming essentials documentation and extend Option/Result functionality
- new public API — Curry, Apply, Compose, ToFunc

### Fixed

- update conventions for async handling and error management [skip ci]
- change the MinVerDefaultPreReleaseIdentifiers to preview.0; separate artifacts output layout settings with a comment
- instead of examples/*.lock.json -> examples/packages.lock.json
- remove remove_traps
- addressed Copilot's comments:

### Internal

- initial scaffold
- Bump the minor-and-patch group with 1 update
- add first programming constructs and enhance error handling
- update Copilot instructions and adjust package project path in CI.yaml
- remove unused Map and Bind methods from NoneType, enhance Option struct with equality operators and tests
- enhance conventions for instance and extension methods [skip ci]
- update conventions to include interface honesty and AI co-authorship guidelines [skip ci]
- minor cosmetic changes in the tests
- enhance XML documentation for Result<T> to clarify type constraints and usage
- Add comprehensive tests for Option and Result monads
- add Bind tests for Option<T> and Result<T> to verify function invocation behavior
- add unit tests for Result and Option extensions
- rename DefaultError to DefaultResultError and update references throughout the codebase
- update conventions to clarify rules on exceptions and testing requirements for value types and monadic types
- update package versions for Microsoft.SourceLink.GitHub, Microsoft.Extensions.TimeProvider.Testing, and Microsoft.Testing.Extensions.TrxReport across multiple files
- update package versions for various dependencies across multiple files
- remove unused package-projects-len output from workflows
- update vm2.TestUtilities package version to 2.1.4
- drop UTF-8 BOM from *.cs files; standardize on charset = utf-8
- after CI big bang
- diff-shared
- update-packages
- update vm2.TestUtilities to version 2.1.7
- diff-shared CONVENTIONS.md
- upgrade .NET SDK to 10.0.402 and update packages
- update lock files

All notable changes to this project will be documented in this file.

## [0.0.0] - YYYY-MM-DD

- Initial scaffold created from vm2pkg template.

## Usage Notes

> [!TIP] Be disciplined with your commit messages and let git-cliff do the work of updating this file.
>
> **Added:**
>
> - add new features
> - commit prefix for git-cliff: `feat:`
>
> **Changed:**
>
> - add behavior changes
> - commit prefix for git-cliff: `refactor:`
>
> **Fixed:**
>
> - add bug fixes
> - commit prefix for git-cliff: `fix:`
>
> **Performance**
>
> - add performance improvements
> - commit prefix for git-cliff: `perf:`
>
> **Security**
>
> - add security-related changes
> - commit prefix for git-cliff: `security:`
>
> **Remove or Revert**
>
> - add removed/obsolete items
> - commit prefix for git-cliff: `revert:` or `remove:`
>
> **Internal**
>
> - add internal changes
> - commit prefix for git-cliff: `refactor:`, `doc:`, `docs:`, `style:`, `test:`, `tests:`, `chore:`, `ci:`, `build:`
>

## References

This format follows:

- [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
- [Semantic Versioning](https://semver.org/)
- Version numbers are produced by [MinVer](./ReleaseProcess.md) from Git tags.
