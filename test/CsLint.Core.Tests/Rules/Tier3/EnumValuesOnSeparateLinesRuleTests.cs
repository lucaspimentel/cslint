using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EnumValuesOnSeparateLinesRuleTests
{
    private readonly EnumValuesOnSeparateLinesRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_enum_values_on_separate_lines"] = "true",
        });

    [Fact]
    public void Analyze_EnumValuesOnSameLine_ReturnsDiagnostic()
    {
        string source = "enum Color { Red, Green, Blue }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, d => Assert.Equal("CSLINT246", d.RuleId));
    }

    [Fact]
    public void Analyze_TwoValuesOnSameLine_ReturnsSingleDiagnostic()
    {
        string source = """
            enum Color
            {
                Red, Green,
                Blue
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT246", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_EnumValuesOnSeparateLines_ReturnsNoDiagnostics()
    {
        string source = """
            enum Color
            {
                Red,
                Green,
                Blue
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleEnumValue_ReturnsNoDiagnostics()
    {
        string source = "enum Color { Red }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "enum Color { Red, Green, Blue }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_enum_values_on_separate_lines"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
