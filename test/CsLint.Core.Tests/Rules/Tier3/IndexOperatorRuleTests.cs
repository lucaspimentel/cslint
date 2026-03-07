using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class IndexOperatorRuleTests
{
    private readonly IndexOperatorRule _rule = new();

    [Theory]
    [InlineData("arr[arr.Length - 1]", "Length")]
    [InlineData("list[list.Count - 1]", "Count")]
    public void Analyze_LengthMinusN_ReturnsDiagnostic(string access, string property)
    {
        string source = $$"""
            class C
            {
                void M(int[] arr, System.Collections.Generic.List<int> list)
                {
                    var x = {{access}};
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_index_operator"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT226", diagnostics[0].RuleId);
        Assert.Contains(property, diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_DirectIndex_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int[] arr)
                {
                    var x = arr[0];
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_index_operator"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_DifferentReceiver_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int[] arr, int[] other)
                {
                    var x = arr[other.Length - 1];
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_index_operator"] = "true",
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
                void M(int[] arr)
                {
                    var x = arr[arr.Length - 1];
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_index_operator"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
