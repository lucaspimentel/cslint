using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedRuleTests
{
    private readonly ExpressionBodiedRule _rule = new();

    [Fact]
    public void Analyze_ExpressionBodiedMethod_ReturnsNoDiagnostics()
    {
        string source = "class C { int GetValue() => 42; }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_methods"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_BlockBodyMethod_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = "class C { int GetValue() { return 42; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_methods"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT201", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultiStatementMethod_WhenExpressionPreferred_ReturnsNoDiagnostics()
    {
        string source = "class C { int GetValue() { int x = 1; return x; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_methods"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
