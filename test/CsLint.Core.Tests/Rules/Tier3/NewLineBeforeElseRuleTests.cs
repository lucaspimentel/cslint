using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NewLineBeforeElseRuleTests
{
    private readonly NewLineBeforeElseRule _rule = new();

    [Fact]
    public void Analyze_ElseOnSameLine_WhenRequired_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                    } else
                    {
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_else"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT280", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ElseOnNewLine_WhenRequired_ReturnsNoDiagnostics()
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
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_else"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ElseOnNewLine_WhenNotRequired_ReturnsDiagnostic()
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
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_else"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { if (true) { } else { } } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
