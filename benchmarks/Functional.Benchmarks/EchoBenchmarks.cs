// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Functional;

#if SHORT_RUN
[ShortRunJob]
#else
[SimpleJob(RuntimeMoniker.HostProcess)]
#endif
public class EchoBenchmarks
{
    private string _value = "payload";

    [Benchmark]
    public string Echo_Value() => FunctionalApi.Echo(_value, "fallback");

    [Benchmark]
    public string Echo_Fallback() => FunctionalApi.Echo(null, "fallback");
}
