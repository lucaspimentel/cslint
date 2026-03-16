using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class SelfAssignmentRuleTests
{
    private readonly SelfAssignmentRule _rule = new();

    [Fact]
    public void Analyze_LocalSelfAssignment_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 0;
                    x = x;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT304", diagnostic.RuleId);
        Assert.Contains("x", diagnostic.Message);
    }

    [Fact]
    public void Analyze_ParameterSelfAssignment_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                void M(int param)
                {
                    param = param;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT304", diagnostic.RuleId);
        Assert.Contains("param", diagnostic.Message);
    }

    [Fact]
    public void Analyze_FieldSelfAssignment_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                private int _field;

                void M()
                {
                    _field = _field;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT304", diagnostic.RuleId);
        Assert.Contains("_field", diagnostic.Message);
    }

    [Fact]
    public void Analyze_DifferentVariables_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 0;
                    int y = 0;
                    x = y;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_CompoundAssignment_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    x += x;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    private static (RuleContext Context, SemanticModel Model) CreateSemanticContext(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);
        SemanticModel model = CompilationFactory.CreateSemanticModel(context.SyntaxTree);
        return (context, model);
    }
}
