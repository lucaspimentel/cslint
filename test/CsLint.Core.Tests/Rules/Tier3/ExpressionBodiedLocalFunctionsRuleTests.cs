using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedLocalFunctionsRuleTests
{
    private readonly ExpressionBodiedLocalFunctionsRule _rule = new();

    [Fact]
    public void Analyze_LocalFunctionWithBlockReturn_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Add(int a, int b)
                    {
                        return a + b;
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_local_functions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT218", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_LocalFunctionWithExpressionBody_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Add(int a, int b) => a + b;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_local_functions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LocalFunctionWithMultipleStatements_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Add(int a, int b)
                    {
                        var sum = a + b;
                        return sum;
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_local_functions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LocalFunctionWithExpressionStatement_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    void Print(string s)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_local_functions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Add(int a, int b)
                    {
                        return a + b;
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_local_functions"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Add(int a, int b)
                    {
                        return a + b;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
