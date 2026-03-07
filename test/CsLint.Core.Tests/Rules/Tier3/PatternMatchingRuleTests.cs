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

    [Fact]
    public void Analyze_IsCheckOnPropertyWithCastOnDifferentExpression_NoDiagnostic()
    {
        string source = """
            class C
            {
                void M(object info)
                {
                    if (info is string)
                    {
                        var x = ((System.Text.StringBuilder)info).Length;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // The is-check target (info) matches the cast target (info), so this SHOULD flag
        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_IsCheckOnMemberWithCastOnDifferentTarget_NoDiagnostic()
    {
        string source = """
            enum Kind { A, B }
            class Info { public Kind Kind; }
            class Other { }
            class C
            {
                void M(Info info)
                {
                    if (info.Kind is Kind)
                    {
                        var x = (Other)info;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // is-check on info.Kind but cast on info — different expressions, should NOT flag
        Assert.Empty(diagnostics);
    }
}
