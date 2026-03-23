using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class NamingConventionParserTests
{
    [Fact]
    public void Parse_ValidThreePartRule_ReturnsRule()
    {
        var properties = new Dictionary<string, string>
        {
            ["dotnet_naming_rule.my_rule.symbols"] = "my_symbols",
            ["dotnet_naming_rule.my_rule.style"] = "my_style",
            ["dotnet_naming_rule.my_rule.severity"] = "warning",
            ["dotnet_naming_symbols.my_symbols.applicable_kinds"] = "class, struct",
            ["dotnet_naming_symbols.my_symbols.applicable_accessibilities"] = "public",
            ["dotnet_naming_style.my_style.capitalization"] = "pascal_case",
        };

        List<ParsedNamingRule> rules = NamingConventionParser.Parse(properties);

        Assert.Single(rules);
        Assert.Equal("my_rule", rules[0].Rule.Name);
        Assert.Contains("class", rules[0].Symbols.ApplicableKinds);
        Assert.Contains("struct", rules[0].Symbols.ApplicableKinds);
        Assert.Contains("public", rules[0].Symbols.ApplicableAccessibilities);
        Assert.Equal("pascal_case", rules[0].Style.Capitalization);
    }

    [Fact]
    public void Parse_WithPrefixAndSuffix_ParsesCorrectly()
    {
        var properties = new Dictionary<string, string>
        {
            ["dotnet_naming_rule.r.symbols"] = "s",
            ["dotnet_naming_rule.r.style"] = "st",
            ["dotnet_naming_rule.r.severity"] = "error",
            ["dotnet_naming_symbols.s.applicable_kinds"] = "interface",
            ["dotnet_naming_style.st.capitalization"] = "pascal_case",
            ["dotnet_naming_style.st.required_prefix"] = "I",
        };

        List<ParsedNamingRule> rules = NamingConventionParser.Parse(properties);

        Assert.Single(rules);
        Assert.Equal("I", rules[0].Style.RequiredPrefix);
        Assert.Null(rules[0].Style.RequiredSuffix);
    }

    [Fact]
    public void Parse_MissingSymbolsRef_SkipsRule()
    {
        var properties = new Dictionary<string, string>
        {
            ["dotnet_naming_rule.r.style"] = "st",
            ["dotnet_naming_rule.r.severity"] = "warning",
            ["dotnet_naming_style.st.capitalization"] = "pascal_case",
        };

        Assert.Empty(NamingConventionParser.Parse(properties));
    }

    [Fact]
    public void Parse_NoNamingRules_ReturnsEmpty()
    {
        var properties = new Dictionary<string, string>
        {
            ["indent_size"] = "4",
        };

        Assert.Empty(NamingConventionParser.Parse(properties));
    }

    [Theory]
    [InlineData("MyClass", "pascal_case", null, true)]
    [InlineData("myField", "camel_case", null, true)]
    [InlineData("MY_CONST", "all_upper", null, true)]
    [InlineData("myclass", "all_lower", null, true)]
    [InlineData("IMyInterface", "pascal_case", "I", true)]
    [InlineData("MyInterface", "pascal_case", "I", false)]
    [InlineData("myField", "pascal_case", null, false)]
    [InlineData("MyClass", "camel_case", null, false)]
    public void MatchesStyle_VariousCases(
        string name, string capitalization, string? prefix, bool expected)
    {
        var style = new NamingStyleDefinition(capitalization, prefix, null, null);

        Assert.Equal(expected, NamingConventionParser.MatchesStyle(name, style));
    }

    [Fact]
    public void MatchesStyle_WithWordSeparator_ChecksEachWord()
    {
        var style = new NamingStyleDefinition("all_upper", null, null, "_");

        Assert.True(NamingConventionParser.MatchesStyle("MY_CONST_VALUE", style));
        Assert.False(NamingConventionParser.MatchesStyle("my_const_value", style));
    }

    [Fact]
    public void Parse_SpecificityOrdering_MoreSpecificFirst()
    {
        var properties = new Dictionary<string, string>
        {
            // Broad rule: all types
            ["dotnet_naming_rule.all_types.symbols"] = "all",
            ["dotnet_naming_rule.all_types.style"] = "pascal",
            ["dotnet_naming_rule.all_types.severity"] = "warning",
            ["dotnet_naming_symbols.all.applicable_kinds"] = "*",
            ["dotnet_naming_symbols.all.applicable_accessibilities"] = "*",
            // Specific rule: private fields with static modifier
            ["dotnet_naming_rule.static_fields.symbols"] = "statics",
            ["dotnet_naming_rule.static_fields.style"] = "pascal",
            ["dotnet_naming_rule.static_fields.severity"] = "warning",
            ["dotnet_naming_symbols.statics.applicable_kinds"] = "field",
            ["dotnet_naming_symbols.statics.applicable_accessibilities"] = "private",
            ["dotnet_naming_symbols.statics.required_modifiers"] = "static",
            // Shared style
            ["dotnet_naming_style.pascal.capitalization"] = "pascal_case",
        };

        List<ParsedNamingRule> rules = NamingConventionParser.Parse(properties);

        Assert.Equal(2, rules.Count);
        // More specific rule (static private fields) should come first
        Assert.Equal("static_fields", rules[0].Rule.Name);
        Assert.Equal("all_types", rules[1].Rule.Name);
    }
}
