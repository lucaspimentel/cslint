namespace Cslint.Core.Rules.Tier2;

internal static class NamingConventionParser
{
    public static List<ParsedNamingRule> Parse(IReadOnlyDictionary<string, string> properties)
    {
        var ruleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var symbolGroups = new Dictionary<string, NamingSymbolGroup>(StringComparer.OrdinalIgnoreCase);
        var styles = new Dictionary<string, NamingStyleDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (string key in properties.Keys)
        {
            string[] parts = key.Split('.');

            if (parts.Length == 3
                && string.Equals(parts[0], "dotnet_naming_rule", StringComparison.OrdinalIgnoreCase))
            {
                ruleNames.Add(parts[1]);
            }
        }

        if (ruleNames.Count == 0)
        {
            return [];
        }

        foreach (string name in CollectGroupNames(properties, "dotnet_naming_symbols"))
        {
            symbolGroups[name] = ParseSymbolGroup(properties, name);
        }

        foreach (string name in CollectGroupNames(properties, "dotnet_naming_style"))
        {
            styles[name] = ParseStyle(properties, name);
        }

        var result = new List<ParsedNamingRule>();

        foreach (string ruleName in ruleNames)
        {
            string? symbolsRef = GetProperty(properties, $"dotnet_naming_rule.{ruleName}.symbols");
            string? styleRef = GetProperty(properties, $"dotnet_naming_rule.{ruleName}.style");
            string? severity = GetProperty(properties, $"dotnet_naming_rule.{ruleName}.severity");

            if (symbolsRef is null || styleRef is null)
            {
                continue;
            }

            if (!symbolGroups.TryGetValue(symbolsRef, out NamingSymbolGroup? symbolGroup)
                || !styles.TryGetValue(styleRef, out NamingStyleDefinition? style))
            {
                continue;
            }

            var rule = new NamingRuleDefinition(ruleName, symbolsRef, styleRef, severity ?? "warning");
            result.Add(new ParsedNamingRule(rule, symbolGroup, style));
        }

        // Sort by specificity: more specific rules first
        // Specificity = number of non-wildcard constraints
        result.Sort((a, b) => GetSpecificity(b).CompareTo(GetSpecificity(a)));

        return result;
    }

    public static bool MatchesStyle(string name, NamingStyleDefinition style)
    {
        string checkName = name;

        if (style.RequiredPrefix is not null)
        {
            if (!checkName.StartsWith(style.RequiredPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            checkName = checkName[style.RequiredPrefix.Length..];
        }

        if (style.RequiredSuffix is not null)
        {
            if (!checkName.EndsWith(style.RequiredSuffix, StringComparison.Ordinal))
            {
                return false;
            }

            checkName = checkName[..^style.RequiredSuffix.Length];
        }

        if (checkName.Length == 0)
        {
            return true;
        }

        // If word_separator is specified, split and check each word
        if (style.WordSeparator is not null && checkName.Contains(style.WordSeparator))
        {
            string[] words = checkName.Split(style.WordSeparator);
            return words.All(w => w.Length == 0 || MatchesCapitalization(w, style.Capitalization));
        }

        return MatchesCapitalization(checkName, style.Capitalization);
    }

    private static bool MatchesCapitalization(string name, string capitalization) =>
        capitalization.ToLowerInvariant() switch
        {
            "pascal_case" => NamingHelper.IsPascalCase(name),
            "camel_case" => NamingHelper.IsCamelCase(name),
            "first_word_upper" => char.IsUpper(name[0]),
            "all_upper" => NamingHelper.IsUpperCase(name),
            "all_lower" => name.All(c => !char.IsLetter(c) || char.IsLower(c)),
            _ => true,
        };

    private static int GetSpecificity(ParsedNamingRule rule)
    {
        int score = 0;

        if (!rule.Symbols.ApplicableKinds.Contains("*"))
        {
            score += 10 + Math.Max(0, 10 - rule.Symbols.ApplicableKinds.Count);
        }

        if (!rule.Symbols.ApplicableAccessibilities.Contains("*"))
        {
            score += 10;
        }

        score += rule.Symbols.RequiredModifiers.Count * 5;

        return score;
    }

    private static HashSet<string> CollectGroupNames(
        IReadOnlyDictionary<string, string> properties,
        string prefix)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string key in properties.Keys)
        {
            if (key.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = key.Split('.');

                if (parts.Length >= 3)
                {
                    names.Add(parts[1]);
                }
            }
        }

        return names;
    }

    private static NamingSymbolGroup ParseSymbolGroup(
        IReadOnlyDictionary<string, string> properties,
        string name)
    {
        string? kinds = GetProperty(properties, $"dotnet_naming_symbols.{name}.applicable_kinds");
        string? accessibilities = GetProperty(
            properties, $"dotnet_naming_symbols.{name}.applicable_accessibilities");
        string? modifiers = GetProperty(
            properties, $"dotnet_naming_symbols.{name}.required_modifiers");

        return new NamingSymbolGroup(
            ParseCommaSeparated(kinds ?? "*"),
            ParseCommaSeparated(accessibilities ?? "*"),
            ParseCommaSeparated(modifiers ?? ""));
    }

    private static NamingStyleDefinition ParseStyle(
        IReadOnlyDictionary<string, string> properties,
        string name)
    {
        string? capitalization = GetProperty(
            properties, $"dotnet_naming_style.{name}.capitalization");
        string? prefix = GetProperty(
            properties, $"dotnet_naming_style.{name}.required_prefix");
        string? suffix = GetProperty(
            properties, $"dotnet_naming_style.{name}.required_suffix");
        string? separator = GetProperty(
            properties, $"dotnet_naming_style.{name}.word_separator");

        return new NamingStyleDefinition(
            capitalization ?? "pascal_case",
            string.IsNullOrEmpty(prefix) ? null : prefix,
            string.IsNullOrEmpty(suffix) ? null : suffix,
            string.IsNullOrEmpty(separator) ? null : separator);
    }

    private static string? GetProperty(
        IReadOnlyDictionary<string, string> properties, string key) =>
        properties.TryGetValue(key, out string? value) ? value : null;

    private static HashSet<string> ParseCommaSeparated(string value)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string part in value.Split(','))
        {
            string trimmed = part.Trim();

            if (trimmed.Length > 0)
            {
                result.Add(trimmed);
            }
        }

        return result;
    }
}
