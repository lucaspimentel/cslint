using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedConversionOperatorsRuleTests
{
    private readonly ExpressionBodiedConversionOperatorsRule _rule = new();

    [Fact]
    public void Analyze_ExpressionBodiedConversionOperator_ReturnsNoDiagnostics()
    {
        string source = """
            struct Celsius
            {
                public double Degrees;
                public static implicit operator double(Celsius c) => c.Degrees;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_operators"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_BlockBodyConversionOperator_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = """
            struct Celsius
            {
                public double Degrees;
                public static implicit operator double(Celsius c) { return c.Degrees; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_operators"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0023", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultiStatementBlockBody_ReturnsNoDiagnostics()
    {
        string source = """
            struct Celsius
            {
                public double Degrees;
                public static implicit operator double(Celsius c) { var d = c.Degrees; return d; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_operators"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigNotSet_ReturnsNoDiagnostics()
    {
        string source = """
            struct Celsius
            {
                public double Degrees;
                public static implicit operator double(Celsius c) { return c.Degrees; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>());
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
