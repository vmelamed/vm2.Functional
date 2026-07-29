#!/usr/bin/env dotnet

// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

#:property TargetFramework=net10.0
#:project ../src/Functional/Functional.csproj

global using static System.Console;

global using vm2.Functional;

global using static vm2.Functional.Option;
global using static vm2.Functional.Result;

Dictionary<string, string> stars = new() {
    ["Hondo"]              = "john wayne",
    ["Maverick"]           = "tom cruise",
    ["Indiana Jones"]      = "harrison ford",
    ["Dirty Harry"]        = "clint eastwood",
    ["James Bond"]         = "sean connery",
    ["The Godfather"]      = "marlon brando",
    ["Rocky"]              = "sylvester stallone",
    ["Forrest Gump"]       = "tom hanks",
    ["Lawrence of Arabia"] = "peter o'toole",
    ["Batman"]             = "michael keaton",
    ["Superman"]           = "christopher reeve",
    ["Spider-Man"]         = "tobey maguire",
    ["Iron Man"]           = "robert downey jr.",
    ["Captain America"]    = "chris evans",
};

Func<IReadOnlyDictionary<string, string>, string, string> lookupKeyInDictionary =
    (dictionary, key) => dictionary
                            .Lookup(key)
                            .Filter(v => !string.IsNullOrWhiteSpace(v))
                            .Map(v => v.Split(' ')
                                        .AsEnumerable()
                                        .Select(v => v[0..1].ToUpper()+v[1..].ToLower())
                                        .Join())
                            .Match(
                                onSome: v  => "{key} => {v}",
                                onNone: () => "{key} => none"
                            );

var lookupStars = lookupKeyInDictionary.Apply(stars);

WriteLine(lookupStars("Hondo"));
WriteLine(lookupStars("Maverick"));
WriteLine(lookupStars("Indiana Jones"));
WriteLine(lookupStars("Dirty Harry"));
WriteLine(lookupStars("James Bond"));
WriteLine(lookupStars("The Godfather"));
WriteLine(lookupStars("Rocky"));
WriteLine(lookupStars("Forrest Gump"));
WriteLine(lookupStars("Lawrence of Arabia"));
WriteLine(lookupStars("Batman"));
WriteLine(lookupStars("Superman"));
WriteLine(lookupStars("Spider-Man"));
WriteLine(lookupStars("Iron Man"));
WriteLine(lookupStars("Captain America"));


static class Extensions
{
    extension(IReadOnlyDictionary<string, string> dictionary)
    {
        public Option<string> Lookup(string key)
            => dictionary.TryGetValue(key, out var value) ? Some(value) : None;
    }

    extension(IEnumerable<string> sequence)
    {
        public string Join(char separator = ' ')
            => string.Join(separator, sequence);
    }

    extension (string names)
    {
        public string Capitalize(char separator = ' ')
            => names.Split(separator)
                            .AsEnumerable()
                            .Select(w => string.IsNullOrWhiteSpace(w) ? w : w[0..1].ToUpper() + w[1..].ToLower())
                            .Join(separator);
    }
}
