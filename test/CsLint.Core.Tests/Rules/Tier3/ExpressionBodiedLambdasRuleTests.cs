using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedLambdasRuleTests
{
    private readonly ExpressionBodiedLambdasRule _rule = new();

    [Fact]
    public void Analyze_LambdaWithBlockReturnStatement_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = x => { return x + 1; };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_lambdas"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT217", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_LambdaWithBlockExpressionStatement_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action<int> a = x => { Console.WriteLine(x); };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_lambdas"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_LambdaWithExpressionBody_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = x => x + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_lambdas"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LambdaWithMultipleStatements_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = x => { var y = x; return y + 1; };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_lambdas"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = x => { return x + 1; };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_lambdas"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = x => { return x + 1; };
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_WhenOnSingleLine_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = x => { return x + 1; };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_lambdas"] = "when_on_single_line",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }
}
