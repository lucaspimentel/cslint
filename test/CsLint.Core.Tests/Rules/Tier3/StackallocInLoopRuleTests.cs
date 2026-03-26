using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class StackallocInLoopRuleTests
{
    private readonly StackallocInLoopRule _rule = new();

    [Theory]
    [InlineData("for (int i = 0; i < 10; i++) { Span<int> s = stackalloc int[10]; }")]
    [InlineData("foreach (var x in new[] { 1 }) { Span<int> s = stackalloc int[10]; }")]
    [InlineData("while (true) { Span<int> s = stackalloc int[10]; }")]
    [InlineData("do { Span<int> s = stackalloc int[10]; } while (true);")]
    public void Analyze_StackallocInLoop_ReturnsDiagnostic(string loopCode)
    {
        string source = $$"""
            using System;

            class C
            {
                void M()
                {
                    {{loopCode}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2014", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_StackallocOutsideLoop_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    Span<int> s = stackalloc int[10];
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ImplicitStackallocInLoop_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Span<int> s = stackalloc[] { 1, 2, 3 };
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2014", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_StackallocInNestedLoop_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            Span<int> s = stackalloc int[10];
                        }
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2014", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Span<int> s = stackalloc int[10];
                    }
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2014.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
