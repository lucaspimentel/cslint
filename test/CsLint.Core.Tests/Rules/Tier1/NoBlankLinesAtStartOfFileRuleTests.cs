using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class NoBlankLinesAtStartOfFileRuleTests
{
    private readonly NoBlankLinesAtStartOfFileRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_blank_lines_at_start_of_file"] = "true",
        });

    [Fact]
    public void Analyze_SingleBlankLineAtStart_ReturnsDiagnostic()
    {
        string source = "\nclass C { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1517", diagnostics[0].RuleId);
        Assert.Equal(1, diagnostics[0].Line);
    }

    [Fact]
    public void Analyze_MultipleBlankLinesAtStart_ReturnsMultipleDiagnostics()
    {
        string source = "\n\n\nclass C { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(3, diagnostics.Count);
    }

    [Fact]
    public void Analyze_NoBlankLineAtStart_ReturnsNoDiagnostics()
    {
        string source = "class C { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_StartsWithComment_ReturnsNoDiagnostics()
    {
        string source = "// comment\nclass C { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData(null, false)]
    public void IsEnabled_ReturnsExpectedResult(string? configValue, bool expected)
    {
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_blank_lines_at_start_of_file"] = configValue;
        }

        var config = new LintConfiguration(settings);

        Assert.Equal(expected, _rule.IsEnabled(config));
    }
}
