// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents the no-value returned from the function type <see cref="Func{Unit}"/>-s.
/// Replacement for the <see cref="void"/> which is "returned" by <see cref="Action"/>-s.
/// </summary>
public readonly record struct Unit
{
    /// <summary>
    /// Represents the single instance of the Unit type.
    /// </summary>
    public static readonly Unit Instance = default;
}
