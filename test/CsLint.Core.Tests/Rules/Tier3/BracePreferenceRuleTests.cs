using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class BracePreferenceRuleTests
{
    private readonly BracePreferenceRule _rule = new();

    private static LintConfiguration BracesRequired =>
        new(new Dictionary<string, string> { ["csharp_prefer_braces"] = "true" });

    [Theory]
    [InlineData("class C { void M() { if (true) { } } }")]
    [InlineData("class C { void M() { for (;;) { } } }")]
    [InlineData("class C { void M() { while (true) { } } }")]
    public void Analyze_WithBraces_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, BracesRequired);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { void M() { if (true) return; } }")]
    [InlineData("class C { void M() { while (true) return; } }")]
    public void Analyze_WithoutBraces_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, BracesRequired);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT202", diagnostics[0].RuleId);
    }
}
