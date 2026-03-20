using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class KeywordSpacingRuleTests
{
    private readonly KeywordSpacingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_keyword_spacing"] = "true",
        });

    [Theory]
    [InlineData("class C { void M() { if(true) { } } }", "if")]
    [InlineData("class C { void M() { for(;;) { } } }", "for")]
    [InlineData("class C { void M() { while(true) { } } }", "while")]
    public void Analyze_NoSpaceAfterControlKeyword_ReturnsDiagnostic(string source, string keyword)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.Message.Contains(keyword));
    }

    [Theory]
    [InlineData("class C { void M() { if (true) { } } }")]
    [InlineData("class C { void M() { for (;;) { } } }")]
    [InlineData("class C { void M() { while (true) { } } }")]
    public void Analyze_SpaceAfterControlKeyword_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { void M() { var t = typeof (int); } }")]
    public void Analyze_SpaceAfterExpressionKeyword_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.NotEmpty(diagnostics);
    }

    [Theory]
    [InlineData("class C { void M() { var t = typeof(int); } }")]
    public void Analyze_NoSpaceAfterExpressionKeyword_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { void M() { if(true) { } } }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_keyword_spacing"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
