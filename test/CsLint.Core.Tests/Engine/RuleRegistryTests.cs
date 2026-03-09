using Cslint.Core.Engine;

namespace Cslint.Core.Tests.Engine;

public sealed class RuleRegistryTests
{
    [Fact]
    public void GetAliases_ReturnsExpectedMappings()
    {
        IReadOnlyDictionary<string, List<string>> aliases = RuleRegistry.GetAliases();

        // CSLINT201 (ExpressionBodied) should map to IDE0021-IDE0027
        Assert.True(aliases.ContainsKey("CSLINT201"));
        List<string> expressionBodiedAliases = aliases["CSLINT201"];
        Assert.Contains("IDE0021", expressionBodiedAliases);
        Assert.Contains("IDE0027", expressionBodiedAliases);
        Assert.Equal(7, expressionBodiedAliases.Count);
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

        // CSLINT001 (TrailingWhitespace) has no third-party aliases
        Assert.False(aliases.ContainsKey("CSLINT001"));
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
