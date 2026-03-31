using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedConstructorsRuleTests
{
    private readonly ExpressionBodiedConstructorsRule _rule = new();

    [Fact]
    public void Analyze_ExpressionBodiedConstructor_ReturnsNoDiagnostics()
    {
        string source = "class C { int _x; C(int x) => _x = x; }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_BlockBodyConstructor_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = "class C { int _x; C(int x) { _x = x; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0021", diagnostics[0].RuleId);
        Assert.Contains("Constructor", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_MultiStatementConstructor_WhenExpressionPreferred_ReturnsNoDiagnostics()
    {
        string source = "class C { int _x; int _y; C(int x, int y) { _x = x; _y = y; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExpressionBodiedConstructor_WhenBlockPreferred_ReturnsIDE0021()
    {
        string source = "class C { int _x; C(int x) => _x = x; }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_constructors"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0021", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BlockBodyConstructor_WhenBlockPreferred_ReturnsNoDiagnostics()
    {
        string source = "class C { int _x; C(int x) { _x = x; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_constructors"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConstructorWithBaseInitializer_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = "class B { public B(int x) { } } class C : B { int _x; C(int x) : base(x) { _x = x; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0021", diagnostics[0].RuleId);
        Assert.Contains("Constructor", diagnostics[0].Message);
    }
}
