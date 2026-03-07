using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PatternMatchingNotRuleTests
{
    private readonly PatternMatchingNotRule _rule = new();

    [Fact]
    public void Analyze_NegatedIsExpression_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    if (!(o is string)) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_not_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT219", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NegatedIsPatternWithParens_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    if (!(o is string s)) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_not_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_IsNotPattern_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    if (o is not string) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_not_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NegatedNonIsExpression_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(bool b)
                {
                    if (!b) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_not_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    if (!(o is string)) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_not_pattern"] = "false",
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
                void M(object o)
                {
                    if (!(o is string)) { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
