using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class RangeOperatorRuleTests
{
    private readonly RangeOperatorRule _rule = new();

    [Fact]
    public void Analyze_SingleArgSubstring_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(string s)
                {
                    var sub = s.Substring(3);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_range_operator"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT227", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_TwoArgSubstring_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(string s)
                {
                    var sub = s.Substring(1, 3);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_range_operator"] = "true",
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
                void M(string s)
                {
                    var sub = s.Substring(3);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_range_operator"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
