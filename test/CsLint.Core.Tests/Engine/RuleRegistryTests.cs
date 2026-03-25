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

        Assert.True(aliases.ContainsKey("CSLINT102"));
        Assert.Contains("SA1300", aliases["CSLINT102"]);
    }

    [Fact]
    public void GetAliases_RuleWithNoAliases_NotPresent()
    {
        IReadOnlyDictionary<string, List<string>> aliases = RuleRegistry.GetAliases();

        // CSLINT003 (LineEnding) has no third-party aliases
        Assert.False(aliases.ContainsKey("CSLINT003"));
    }

    [Theory]
    [InlineData("SA1027", "CSLINT002")]
    [InlineData("SA1028", "CSLINT001")]
    [InlineData("SA1101", "CSLINT204")]
    [InlineData("SA1121", "IDE0049")]
    [InlineData("SA1124", "CSLINT006")]
    [InlineData("SA1206", "IDE0036")]
    [InlineData("SA1303", "CSLINT105")]
    [InlineData("SA1312", "CSLINT103")]
    [InlineData("SA1400", "IDE0040")]
    [InlineData("SA1500", "IDE0011")]
    [InlineData("SA1503", "CSLINT228")]
    [InlineData("SA1507", "CSLINT008")]
    [InlineData("SA1518", "CSLINT004")]
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

        // Verify no duplicate rule IDs
        string[] ruleIds = registry.Rules.Select(r => r.RuleId).ToArray();
        Assert.Equal(ruleIds.Distinct().Count(), ruleIds.Length);
    }
}
