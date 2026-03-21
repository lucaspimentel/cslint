using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class BraceSpacingRuleTests
{
    private readonly BraceSpacingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_brace_spacing"] = "true",
        });

    [Fact]
    public void Analyze_NoSpaceBeforeOpenBrace_ReturnsDiagnostic()
    {
        string source = "class C{ }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.Message.Contains("preceded"));
    }

    [Fact]
    public void Analyze_CorrectBraceSpacing_ReturnsNoDiagnostics()
    {
        string source = "class C { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_StringInterpolationBraces_ReturnsNoDiagnostics()
    {
        string source = """class C { string M(string name) { return $"Hello {name}!"; } }""";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleInterpolationBraces_ReturnsNoDiagnostics()
    {
        string source = """class C { string M(int a, int b) { return $"{a} + {b} = {a + b}"; } }""";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C{ }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_brace_spacing"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
