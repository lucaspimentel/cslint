using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnusedUsingRuleTests
{
    private readonly UnusedUsingRule _rule = new();

    [Fact]
    public void Analyze_UnusedBclUsing_ReportsDiagnostic()
    {
        // System.IO is unused — only BCL types involved, so CS8019 should be reported
        const string source = """
            using System;
            using System.IO;

            class C
            {
                void M()
                {
                    Console.WriteLine("hello");
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT300", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_UnresolvedType_SuppressesAllCS8019()
    {
        // SomeExternalLibrary can't be resolved (no reference) → CS0246.
        // System.IO is technically unused but CS8019 is unreliable when CS0246 is present.
        const string source = """
            using System;
            using System.IO;
            using SomeExternalLibrary;

            class C
            {
                void M()
                {
                    ExternalClass x = null;
                    Console.WriteLine(x);
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
