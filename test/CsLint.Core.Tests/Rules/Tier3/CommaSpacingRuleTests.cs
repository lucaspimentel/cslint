using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class CommaSpacingRuleTests
{
    private readonly CommaSpacingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_comma_spacing"] = "true",
        });

    [Fact]
    public void Analyze_SpaceBeforeComma_ReturnsDiagnostic()
    {
        string source = "class C { void M(int a , int b) { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.Message.Contains("before"));
    }

    [Fact]
    public void Analyze_NoSpaceAfterComma_ReturnsDiagnostic()
    {
        string source = "class C { void M(int a,int b) { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.Message.Contains("followed"));
    }

    [Fact]
    public void Analyze_CorrectCommaSpacing_ReturnsNoDiagnostics()
    {
        string source = "class C { void M(int a, int b) { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { void M(int a,int b) { } }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_comma_spacing"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("csharp_space_after_comma", "true")]
    [InlineData("csharp_space_before_comma", "false")]
    public void IsEnabled_StandardKey_ReturnsTrue(string key, string value)
    {
        var config = new LintConfiguration(new Dictionary<string, string> { [key] = value });

        Assert.True(_rule.IsEnabled(config));
    }
}
