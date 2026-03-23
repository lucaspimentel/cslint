using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NewLineBeforeCatchRuleTests
{
    private readonly NewLineBeforeCatchRule _rule = new();

    [Fact]
    public void Analyze_CatchOnSameLine_WhenRequired_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    try
                    {
                    } catch
                    {
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_catch"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
        Assert.Equal("CSLINT281", _rule.Analyze(context)[0].RuleId);
    }

    [Fact]
    public void Analyze_CatchOnNewLine_WhenRequired_ReturnsNoDiagnostics()
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
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_catch"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { try { } catch { } } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
