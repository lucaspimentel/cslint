using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SwitchExpressionPreferenceRuleTests
{
    private readonly SwitchExpressionPreferenceRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_prefer_switch_expression"] = "true:warning",
        });

    [Fact]
    public void Analyze_SwitchWithAllReturns_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                string M(int x)
                {
                    switch (x)
                    {
                        case 1: return "one";
                        case 2: return "two";
                        default: return "other";
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0066", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SwitchWithAllAssignments_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    string result;
                    switch (x)
                    {
                        case 1: result = "one"; break;
                        case 2: result = "two"; break;
                        default: result = "other"; break;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_SwitchExpression_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M(int x) => x switch
                {
                    1 => "one",
                    2 => "two",
                    _ => "other",
                };
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SwitchWithSideEffects_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1: System.Console.WriteLine("one"); break;
                        case 2: System.Console.WriteLine("two"); break;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SwitchWithMixedReturnAndAssignment_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M(int x)
                {
                    string result;
                    switch (x)
                    {
                        case 1: return "one";
                        default: result = "other"; break;
                    }
                    return result;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SwitchWithDifferentAssignTargets_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    string a;
                    string b;
                    switch (x)
                    {
                        case 1: a = "one"; break;
                        default: b = "other"; break;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SingleSection_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M(int x)
                {
                    switch (x)
                    {
                        default: return "only";
                    }
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
                string M(int x)
                {
                    switch (x)
                    {
                        case 1: return "one";
                        default: return "other";
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
