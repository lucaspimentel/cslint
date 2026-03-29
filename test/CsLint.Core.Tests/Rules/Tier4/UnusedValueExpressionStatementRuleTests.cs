using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnusedValueExpressionStatementRuleTests
{
    private readonly UnusedValueExpressionStatementRule _rule = new();

    [Fact]
    public void Analyze_UnusedMethodReturnValue_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                int GetValue() => 42;

                void M()
                {
                    GetValue();
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0058", diagnostic.RuleId);
        Assert.Contains("int", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UnusedToString_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 42;
                    x.ToString();
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0058", diagnostic.RuleId);
        Assert.Contains("string", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UnusedObjectCreation_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    new object();
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0058", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_VoidMethodCall_NoDiagnostic()
    {
        const string source = """
            using System;

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

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AssignedReturnValue_NoDiagnostic()
    {
        const string source = """
            class C
            {
                int GetValue() => 42;

                void M()
                {
                    var x = GetValue();
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_DiscardedReturnValue_NoDiagnostic()
    {
        const string source = """
            class C
            {
                int GetValue() => 42;

                void M()
                {
                    _ = GetValue();
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("i++")]
    [InlineData("i--")]
    [InlineData("++i")]
    [InlineData("--i")]
    public void Analyze_IncrementDecrement_NoDiagnostic(string expression)
    {
        string source = $$"""
            class C
            {
                void M()
                {
                    int i = 0;
                    {{expression}};
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AwaitVoidTask_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;

            class C
            {
                async Task M()
                {
                    await Task.Delay(1);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AwaitTaskWithResult_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;

            class C
            {
                async Task M()
                {
                    await Task.FromResult(42);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0058", diagnostic.RuleId);
        Assert.Contains("int", diagnostic.Message);
    }

    private static (RuleContext Context, SemanticModel Model) CreateSemanticContext(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);
        SemanticModel model = CompilationFactory.CreateSemanticModel(context.SyntaxTree);
        return (context, model);
    }
}
