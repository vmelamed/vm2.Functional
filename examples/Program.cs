#!/usr/bin/env dotnet

// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

#:property TargetFramework=net10.0
#:project ../src/Functional/Functional.csproj

using static System.Console;

using vm2.Functional;

WriteLine("Functional example");
WriteLine(FunctionalApi.Echo("hello", "fallback"));
WriteLine(FunctionalApi.Echo(null, "fallback"));
