using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class BlankLineAfterBlockRuleTests
{
    private readonly BlankLineAfterBlockRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_block"] = "false",
        });

    [Fact]
    public void Analyze_BlockFollowedByStatementNoBlankLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        return;
                    }
                    int x = 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT230", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BlockFollowedByBlankLineThenStatement_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        return;
                    }

                    int x = 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IfElse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                    }
                    else
                    {
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TryCatchFinally_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    try
                    {
                    }
                    catch
                    {
                    }
                    finally
                    {
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LastStatementInBlock_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        return;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAllowed_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        return;
                    }
                    int x = 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_block"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        return;
                    }
                    int x = 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
