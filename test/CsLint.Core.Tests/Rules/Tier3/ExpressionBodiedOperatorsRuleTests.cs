using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedOperatorsRuleTests
{
    private readonly ExpressionBodiedOperatorsRule _rule = new();

    [Fact]
    public void Analyze_ExpressionBodiedOperator_ReturnsNoDiagnostics()
    {
        string source = """
            struct Point
            {
                public int X;
                public int Y;
                public static Point operator +(Point a, Point b) => new Point { X = a.X + b.X, Y = a.Y + b.Y };
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
    public void Analyze_BlockBodyOperator_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = """
            struct Point
            {
                public int X;
                public int Y;
                public static Point operator +(Point a, Point b) { return new Point { X = a.X + b.X, Y = a.Y + b.Y }; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_operators"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0024", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultiStatementBlockBody_ReturnsNoDiagnostics()
    {
        string source = """
            struct Point
            {
                public int X;
                public int Y;
                public static Point operator +(Point a, Point b) { var p = new Point(); p.X = a.X + b.X; return p; }
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
            struct Point
            {
                public int X;
                public int Y;
                public static Point operator +(Point a, Point b) { return new Point { X = a.X + b.X, Y = a.Y + b.Y }; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>());
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
