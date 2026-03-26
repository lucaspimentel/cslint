using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class DuplicateEnumValueRuleTests
{
    private readonly DuplicateEnumValueRule _rule = new();

    [Fact]
    public void Analyze_ExplicitDuplicateValues_ReportsDiagnostic()
    {
        const string source = """
            enum Color
            {
                Red = 1,
                Blue = 1,
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1069", diagnostic.RuleId);
        Assert.Contains("Blue", diagnostic.Message);
        Assert.Contains("Red", diagnostic.Message);
    }

    [Fact]
    public void Analyze_ImplicitDuplicateValues_ReportsDiagnostic()
    {
        const string source = """
            enum Numbers
            {
                A = 0,
                B,
                C = 1,
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1069", diagnostic.RuleId);
        Assert.Contains("C", diagnostic.Message);
        Assert.Contains("B", diagnostic.Message);
    }

    [Fact]
    public void Analyze_AllUniqueValues_NoDiagnostic()
    {
        const string source = """
            enum Direction
            {
                Up,
                Down,
                Left,
                Right,
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExpressionDuplicateValues_ReportsDiagnostic()
    {
        const string source = """
            enum Flags
            {
                A = 1 << 0,
                B = 1,
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1069", diagnostic.RuleId);
        Assert.Contains("B", diagnostic.Message);
        Assert.Contains("A", diagnostic.Message);
    }

    private static (RuleContext Context, SemanticModel Model) CreateSemanticContext(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);
        SemanticModel model = CompilationFactory.CreateSemanticModel(context.SyntaxTree);
        return (context, model);
    }
}
