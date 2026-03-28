using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ClosingBraceSpacingRuleTests
{
    private readonly ClosingBraceSpacingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_brace_spacing"] = "true",
        });

    [Fact]
    public void Analyze_NoSpaceAfterCloseBrace_ReturnsDiagnostic()
    {
        string source = "class C { void M() { }void N() { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.RuleId == "SA1013" && d.Message.Contains("followed"));
    }

    [Fact]
    public void Analyze_CorrectBraceSpacing_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_StringInterpolationBraces_ReturnsNoDiagnostics()
    {
        string source = """class C { string M(string name) { return $"Hello {name}!"; } }""";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_CloseBraceFollowedBySemicolon_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { int x = 1; } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { void M() { }void N() { } }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_brace_spacing"] = configValue;
        }

        RuleContext context = TestHelper.CreateContext(source, new LintConfiguration(settings));

        Assert.Empty(_rule.Analyze(context));
    }
}
