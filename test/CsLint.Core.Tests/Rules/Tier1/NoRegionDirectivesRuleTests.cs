using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class NoRegionDirectivesRuleTests
{
    private readonly NoRegionDirectivesRule _rule = new();

    [Theory]
    [InlineData("class Foo { }")]
    [InlineData("class Foo { }\n")]
    [InlineData("// comment about regions\n")]
    [InlineData("string s = \"#region not a directive\";\n")]
    public void Analyze_NoRegionDirectives_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("#region MyRegion\n")]
    [InlineData("  #region MyRegion\n")]
    [InlineData("\t#region MyRegion\n")]
    public void Analyze_RegionDirective_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT006", diagnostics[0].RuleId);
        Assert.Equal(1, diagnostics[0].Line);
    }

    [Theory]
    [InlineData("#endregion\n")]
    [InlineData("  #endregion\n")]
    public void Analyze_EndRegionDirective_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT006", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_RegionAndEndRegion_ReturnsTwoDiagnostics()
    {
        string source = "#region Methods\nvoid Foo() { }\n#endregion\n";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.Equal(1, diagnostics[0].Line);
        Assert.Equal(3, diagnostics[1].Line);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsEnabled_RespectsConfiguration(bool configValue, bool expected)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["csharp_no_region_directives"] = configValue.ToString().ToLowerInvariant() });

        Assert.Equal(expected, _rule.IsEnabled(config));
    }
}
