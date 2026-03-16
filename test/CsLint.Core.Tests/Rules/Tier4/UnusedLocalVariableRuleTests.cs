using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnusedLocalVariableRuleTests
{
    private readonly UnusedLocalVariableRule _rule = new();

    [Fact]
    public void Analyze_UnusedLocal_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 42;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT301", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_UsedLocal_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    int x = 42;
                    Console.WriteLine(x);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UnresolvedType_SuppressesAllCS0219()
    {
        const string source = """
            class C
            {
                void M()
                {
                    ExternalType x = null;
                    int y = 42;
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
