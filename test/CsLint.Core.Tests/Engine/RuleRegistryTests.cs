using Cslint.Core.Engine;

namespace Cslint.Core.Tests.Engine;

public sealed class RuleRegistryTests
{
    [Fact]
    public void GetAliases_ReturnsExpectedMappings()
    {
        IReadOnlyDictionary<string, List<string>> aliases = RuleRegistry.GetAliases();

        // IDE0021 (ExpressionBodiedMethods) should have a self-referencing alias
        Assert.True(aliases.ContainsKey("IDE0021"));
        Assert.Contains("IDE0021", aliases["IDE0021"]);
    }

    [Fact]
    public void GetAliases_ContainsStyleCopMappings()
    {
        IReadOnlyDictionary<string, List<string>> aliases = RuleRegistry.GetAliases();

        Assert.True(aliases.ContainsKey("SA1300"));
        Assert.Contains("SA1300", aliases["SA1300"]);
    }

    [Fact]
    public void GetAliases_RuleWithNoAliases_NotPresent()
    {
        IReadOnlyDictionary<string, List<string>> aliases = RuleRegistry.GetAliases();

        // CSLINT003 (LineEnding) has no third-party aliases
        Assert.False(aliases.ContainsKey("CSLINT003"));
    }

    [Theory]
    [InlineData("SA1027", "SA1027")]
    [InlineData("SA1028", "SA1028")]
    [InlineData("SA1101", "SA1101")]
    [InlineData("SA1121", "IDE0049")]
    [InlineData("SA1124", "SA1124")]
    [InlineData("SA1206", "IDE0036")]
    [InlineData("SA1303", "SA1303")]
    [InlineData("SA1312", "SA1312")]
    [InlineData("SA1400", "IDE0040")]
    [InlineData("SA1500", "IDE0011")]
    [InlineData("SA1503", "SA1503")]
    [InlineData("SA1507", "SA1507")]
    [InlineData("SA1518", "SA1518")]
    public void GetAliases_ContainsNewStyleCopMappings(string styleCopId, string cslintId)
    {
        IReadOnlyDictionary<string, List<string>> aliases = RuleRegistry.GetAliases();

        Assert.True(aliases.ContainsKey(cslintId));
        Assert.Contains(styleCopId, aliases[cslintId]);
    }

    [Fact]
    public void CreateDefault_RegistersAllRules()
    {
        RuleRegistry registry = RuleRegistry.CreateDefault();

        Assert.True(registry.Rules.Count > 0);

        // Verify no duplicate rule names (IDs may be shared, e.g., IDE0021 for methods and constructors)
        string[] ruleNames = registry.Rules.Select(r => r.Name).ToArray();
        Assert.Equal(ruleNames.Distinct().Count(), ruleNames.Length);
    }
}
