using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class OpeningParenthesisSpacingRuleTests
{
    private readonly OpeningParenthesisSpacingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_parenthesis_spacing"] = "true",
        });

    [Fact]
    public void Analyze_SpaceAfterOpenParen_ReturnsDiagnostic()
    {
        string source = "class C { void M( int x) { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.RuleId == "SA1008" && d.Message.Contains("opening"));
    }

    [Fact]
    public void Analyze_CorrectParenSpacing_ReturnsNoDiagnostics()
    {
        string source = "class C { void M(int x) { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_EmptyParens_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { void M( int x) { } }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_parenthesis_spacing"] = configValue;
        }

        RuleContext context = TestHelper.CreateContext(source, new LintConfiguration(settings));

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void IsEnabled_StandardKeyFalse_ReturnsTrue()
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_between_parentheses"] = "false",
        });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_StandardKeyWithContexts_ReturnsFalse()
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_between_parentheses"] = "control_flow_statements",
        });

        Assert.False(_rule.IsEnabled(config));
    }
}
