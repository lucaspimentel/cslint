using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConditionalDelegateCallRuleTests
{
    private readonly ConditionalDelegateCallRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_conditional_delegate_call"] = "true:warning",
        });

    [Fact]
    public void Analyze_NullCheckThenInvoke_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(System.Action handler)
                {
                    if (handler != null)
                    {
                        handler();
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE1005", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NullCheckThenInvokeNoBraces_ReturnsDiagnostic()
    {
        string source = "class C { void M(System.Action h) { if (h != null) h(); } }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ReversedNullCheck_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(System.Action handler)
                {
                    if (null != handler)
                    {
                        handler();
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConditionalInvoke_ReturnsNoDiagnostics()
    {
        string source = "class C { void M(System.Action h) { h?.Invoke(); } }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NullCheckWithElse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(System.Action handler)
                {
                    if (handler != null)
                    {
                        handler();
                    }
                    else
                    {
                        return;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NullCheckDifferentInvocation_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(System.Action handler, System.Action other)
                {
                    if (handler != null)
                    {
                        other();
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NullCheckNonInvocationBody_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(System.Action handler)
                {
                    if (handler != null)
                    {
                        int x = 1;
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
                void M(System.Action h) { if (h != null) h(); }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
