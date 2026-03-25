using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedPropertiesRuleTests
{
    private readonly ExpressionBodiedPropertiesRule _rule = new();

    [Fact]
    public void Analyze_SingleGetterBlock_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = "class C { int Value { get { return 42; } } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_properties"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0025", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ExpressionBodiedProperty_ReturnsNoDiagnostics()
    {
        string source = "class C { int Value => 42; }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_properties"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_GetSetProperty_ReturnsNoDiagnostics()
    {
        string source = "class C { int _v; int Value { get { return _v; } set { _v = value; } } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_properties"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
