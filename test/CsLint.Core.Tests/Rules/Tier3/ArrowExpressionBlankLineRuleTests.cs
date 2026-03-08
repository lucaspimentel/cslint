using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ArrowExpressionBlankLineRuleTests
{
    private readonly ArrowExpressionBlankLineRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_token_in_arrow_expression_clause"] = "false",
        });

    [Fact]
    public void Analyze_BlankLineAfterArrow_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int M() =>

                    42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT233", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ArrowSameLine_ReturnsNoDiagnostics()
    {
        string source = "class C { int M() => 42; }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ArrowNextLine_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                int M() =>
                    42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PropertyArrowBlankLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int P =>

                    42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT233", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ConfigAllowed_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                int M() =>

                    42;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_token_in_arrow_expression_clause"] = "true",
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
                int M() =>

                    42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
