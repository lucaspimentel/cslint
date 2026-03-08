using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConditionalExpressionBlankLineRuleTests
{
    private readonly ConditionalExpressionBlankLineRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_token_in_conditional_expression"] = "false",
        });

    [Fact]
    public void Analyze_BlankLineAfterQuestionMark_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int M(bool b) => b ?

                    1 : 2;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT232", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BlankLineAfterColon_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int M(bool b) => b ?
                    1 :

                    2;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT232", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BlankLineAfterBoth_ReturnsTwoDiagnostics()
    {
        string source = """
            class C
            {
                int M(bool b) => b ?

                    1 :

                    2;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void Analyze_MultilineNoBlankLines_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                int M(bool b) => b
                    ? 1
                    : 2;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleLine_ReturnsNoDiagnostics()
    {
        string source = "class C { int M(bool b) => b ? 1 : 2; }";
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
                int M(bool b) => b ?

                    1 : 2;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_token_in_conditional_expression"] = "true",
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
                int M(bool b) => b ?

                    1 : 2;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
