using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class IndentationRuleTests
{
    private readonly IndentationRule _rule = new();

    [Theory]
    [InlineData("    int x;", "space", "4")]
    [InlineData("        int x;", "space", "4")]
    [InlineData("\tint x;", "tab", null)]
    public void Analyze_CorrectIndentation_ReturnsNoDiagnostics(string source, string style, string? size)
    {
        RuleContext context = CreateContext(source, style, size);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TabsWhenSpacesExpected_ReturnsDiagnostic()
    {
        RuleContext context = CreateContext("\tint x;", "space", "4");

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT002", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SpacesWhenTabsExpected_ReturnsDiagnostic()
    {
        RuleContext context = CreateContext("    int x;", "tab", null);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_OddIndentSize_ReturnsNoDiagnostics()
    {
        // Alignment/continuation indentation should not be flagged
        RuleContext context = CreateContext("   int x;", "space", "4");

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsEnabled_RespectsConfiguration(bool hasConfig, bool expected)
    {
        var props = new Dictionary<string, string>();

        if (hasConfig)
        {
            props["indent_style"] = "space";
        }

        var config = new LintConfiguration(props);

        Assert.Equal(expected, _rule.IsEnabled(config));
    }

    private static RuleContext CreateContext(string source, string style, string? size)
    {
        var props = new Dictionary<string, string> { ["indent_style"] = style };

        if (size is not null)
        {
            props["indent_size"] = size;
        }

        return TestHelper.CreateContext(source, new LintConfiguration(props));
    }
}
