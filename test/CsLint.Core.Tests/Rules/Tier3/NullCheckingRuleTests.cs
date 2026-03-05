using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NullCheckingRuleTests
{
    private readonly NullCheckingRule _rule = new();

    [Fact]
    public void Analyze_CoalesceOperator_ReturnsNoDiagnostics()
    {
        string source = "class C { void M(string s) { var x = s ?? \"\"; } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NullCheckConditional_ReturnsDiagnostic()
    {
        string source = "class C { void M(string s) { var x = s != null ? s : \"\"; } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT210", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NullCheckIfThrow_ReturnsDiagnostic()
    {
        string source = """
            class C {
                void M(string s) {
                    if (s == null) { throw new System.ArgumentNullException(); }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT210", diagnostics[0].RuleId);
    }
}
