using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NewLineBeforeFinallyRuleTests
{
    private readonly NewLineBeforeFinallyRule _rule = new();

    [Fact]
    public void Analyze_FinallyOnSameLine_WhenRequired_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    try
                    {
                    } finally
                    {
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_finally"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
        Assert.Equal("CSLINT282", _rule.Analyze(context)[0].RuleId);
    }

    [Fact]
    public void Analyze_FinallyOnNewLine_WhenRequired_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    try
                    {
                    }
                    finally
                    {
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_finally"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { try { } finally { } } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SingleLineTryFinally_WhenRequired_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { try { } finally { } } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_finally"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // No newline at all — nothing to enforce
        Assert.Empty(_rule.Analyze(context));
    }
}
