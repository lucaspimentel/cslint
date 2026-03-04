using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PatternMatchingRuleTests
{
    private readonly PatternMatchingRule _rule = new();

    [Fact]
    public void Analyze_PatternMatching_ReturnsNoDiagnostics()
    {
        string source = "class C { void M(object o) { if (o is string s) { } } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IsCheckWithCast_ReturnsDiagnostic()
    {
        string source = "class C { void M(object o) { if (o is string) { var s = (string)o; } } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT209", diagnostics[0].RuleId);
    }
}
