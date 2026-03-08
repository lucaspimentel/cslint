using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConsecutiveBracesRuleTests
{
    private readonly ConsecutiveBracesRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_lines_between_consecutive_braces"] = "false",
        });

    [Fact]
    public void Analyze_BlankLineBetweenClosingBraces_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                    }

                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT229", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NoBlankLineBetweenClosingBraces_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
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
    public void Analyze_MultipleNestedBlanks_ReportsEach()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        while (true)
                        {
                        }

                    }

                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
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
                    }

                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_lines_between_consecutive_braces"] = "true",
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
                    }

                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
