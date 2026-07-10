// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Functional;

/// <summary>
/// Represents the absence of value. Replace <see langword="null"/> with <c>NoneType.None</c>.
/// </summary>
public readonly record struct NoneType()
{
    /// <summary>
    /// The only value of <see cref="NoneType"/>. This is the value that represents the absence of a value.
    /// </summary>
    public static readonly NoneType None = default;
}
