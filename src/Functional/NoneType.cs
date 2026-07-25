// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents the absence of value.
/// </summary>
/// <remarks>
/// Replace <see langword="null"/> with <c>default(NoneType)</c>, or with <see cref="Option.None"/>, or just <c>None</c>, if you
/// have <c>global using static vm2.Functional.Option;</c> in scope.
/// </remarks>
public readonly record struct NoneType;
