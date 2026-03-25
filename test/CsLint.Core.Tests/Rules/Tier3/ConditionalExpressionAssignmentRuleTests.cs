using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConditionalExpressionAssignmentRuleTests
{
    private readonly ConditionalExpressionAssignmentRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_conditional_expression_over_assignment"] = "true:warning",
        });

    [Fact]
    public void Analyze_IfElseAssignSameVariable_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(bool b)
                {
                    int x;
                    if (b)
                        x = 1;
                    else
                        x = 2;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0045", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_IfElseAssignWithBlocks_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(bool b)
                {
                    int x;
                    if (b)
                    {
                        x = 1;
                    }
                    else
                    {
                        x = 2;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_TernaryAssignment_ReturnsNoDiagnostics()
    {
        string source = "class C { void M(bool b) { int x = b ? 1 : 2; } }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_IfElseAssignDifferentVariables_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(bool b)
                {
                    int x;
                    int y;
                    if (b)
                        x = 1;
                    else
                        y = 2;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_IfWithoutElse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(bool b)
                {
                    int x = 0;
                    if (b) x = 1;
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
                void M(bool b) { int x; if (b) x = 1; else x = 2; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
