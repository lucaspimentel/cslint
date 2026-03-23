using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class IndentationFormattingRuleTests
{
    private readonly IndentationFormattingRule _rule = new();

    [Fact]
    public void Analyze_SwitchLabelsNotIndented_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                    case 1:
                        break;
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_indent_switch_labels"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT292", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SwitchLabelsIndented_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                            break;
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_indent_switch_labels"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_CaseContentsNotIndented_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                        break;
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_indent_case_contents"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_LabelNotFlushLeft_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    myLabel:
                    return;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_indent_labels"] = "flush_left",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_LabelFlushLeft_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
            myLabel:
                    return;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_indent_labels"] = "flush_left",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                    case 1:
                    break;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
