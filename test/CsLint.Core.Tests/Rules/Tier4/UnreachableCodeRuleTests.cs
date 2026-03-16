using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnreachableCodeRuleTests
{
    private readonly UnreachableCodeRule _rule = new();

    [Fact]
    public void Analyze_UnreachableCode_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                int M()
                {
                    return 1;
                    int x = 2;
                    return x;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, d => Assert.Equal("CSLINT302", d.RuleId));
    }

    [Fact]
    public void Analyze_AllReachable_NoDiagnostic()
    {
        const string source = """
            class C
            {
                int M(bool flag)
                {
                    if (flag)
                        return 1;

                    return 2;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UnresolvedType_SuppressesAllCS0162()
    {
        const string source = """
            class C
            {
                int M()
                {
                    ExternalType x = null;
                    return 1;
                    return 2;
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
