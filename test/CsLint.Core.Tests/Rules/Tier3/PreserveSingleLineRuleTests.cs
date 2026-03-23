using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PreserveSingleLineRuleTests
{
    private readonly PreserveSingleLineRule _rule = new();

    [Fact]
    public void Analyze_SingleLineStatement_WhenNotAllowed_ReturnsDiagnostic()
    {
        string source = "class C { void M() { if (true) return; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_preserve_single_line_statements"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT293", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SingleLineStatement_WhenAllowed_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { if (true) return; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_preserve_single_line_statements"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_MultiLineStatement_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    if (true)
                        return;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_preserve_single_line_statements"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SingleLineBlock_WhenNotAllowed_ReturnsDiagnostic()
    {
        string source = "class C { void M() { if (true) { return; } } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_preserve_single_line_blocks"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.NotEmpty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_MultiLineBlock_ReturnsNoDiagnostics()
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
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_preserve_single_line_blocks"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { if (true) return; } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
