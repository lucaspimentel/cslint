using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnusedValueAssignmentRuleTests
{
    private readonly UnusedValueAssignmentRule _rule = new();

    [Fact]
    public void Analyze_ValueOverwrittenBeforeRead_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                int Compute() => 1;
                int Compute2() => 2;
                void Use(int x) { }

                void M()
                {
                    int x = Compute();
                    x = Compute2();
                    Use(x);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0059", diagnostic.RuleId);
        Assert.Contains("x", diagnostic.Message);
    }

    [Fact]
    public void Analyze_MultipleOverwritesBeforeRead_ReportsAll()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    x = 2;
                    x = 3;
                    System.Console.WriteLine(x);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, d => Assert.Equal("IDE0059", d.RuleId));
    }

    [Fact]
    public void Analyze_ValueReadBeforeReassignment_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void Use(int x) { }

                void M()
                {
                    int x = 1;
                    Use(x);
                    x = 2;
                    Use(x);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ValueUsedNoReassignment_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 42;
                    System.Console.WriteLine(x);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleAssignmentNeverRead_NoDiagnostic()
    {
        // CS0219 territory — only one assignment, so IDE0059 should not flag
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

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_DiscardVariable_NoDiagnostic()
    {
        const string source = """
            class C
            {
                int Compute() => 1;

                void M()
                {
                    _ = Compute();
                    _ = Compute();
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_VariableUsedInIfBranch_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M(bool condition)
                {
                    int x = 1;
                    x = 2;
                    if (condition)
                    {
                        System.Console.WriteLine(x);
                    }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        // The first assignment (x = 1) should be flagged since x = 2 overwrites before branch
        // But x = 2 should NOT be flagged because it's read inside the if
        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Contains("x", diagnostic.Message);
    }

    [Fact]
    public void Analyze_VariableUsedInLoop_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 0;
                    x = 1;
                    while (x < 10)
                    {
                        x++;
                    }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        // x = 0 flagged (overwritten by x = 1 before read)
        // x = 1 NOT flagged (read in while condition)
        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Contains("x", diagnostic.Message);
    }

    [Fact]
    public void Analyze_ForeachVariable_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    var items = new[] { 1, 2, 3 };
                    foreach (var item in items)
                    {
                        System.Console.WriteLine(item);
                    }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_RightHandSideReadsVariable_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    x = x + 1;
                    System.Console.WriteLine(x);
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ReturnReadsVariable_NoDiagnostic()
    {
        const string source = """
            class C
            {
                int M()
                {
                    int x = 1;
                    x = 2;
                    return x;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        // x = 1 flagged (overwritten by x = 2), but x = 2 is read by return
        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Contains("x", diagnostic.Message);
    }

    private static (RuleContext Context, SemanticModel Model) CreateSemanticContext(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);
        SemanticModel model = CompilationFactory.CreateSemanticModel(context.SyntaxTree);
        return (context, model);
    }
}
