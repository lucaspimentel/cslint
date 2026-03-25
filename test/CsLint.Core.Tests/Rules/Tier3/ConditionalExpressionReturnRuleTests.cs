using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConditionalExpressionReturnRuleTests
{
    private readonly ConditionalExpressionReturnRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_conditional_expression_over_return"] = "true:warning",
        });

    [Fact]
    public void Analyze_IfElseReturn_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int M(bool b)
                {
                    if (b)
                        return 1;
                    else
                        return 2;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0046", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_IfElseReturnWithBlocks_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int M(bool b)
                {
                    if (b)
                    {
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_TernaryReturn_ReturnsNoDiagnostics()
    {
        string source = "class C { int M(bool b) => b ? 1 : 2; }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_IfReturnWithoutElse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                int M(bool b)
                {
                    if (b) return 1;
                    return 2;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_IfElseWithVoidReturn_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(bool b)
                {
                    if (b)
                        return;
                    else
                        return;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                int M(bool b) { if (b) return 1; else return 2; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
