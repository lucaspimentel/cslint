using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class TupleSwapRuleTests
{
    private readonly TupleSwapRule _rule = new();

    [Fact]
    public void Analyze_TempVariableSwap_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(int a, int b)
                {
                    var t = a;
                    a = b;
                    b = t;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_tuple_swap"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT223", diagnostics[0].RuleId);
        Assert.Contains("tuple swap", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_TempUsedAfterSwap_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int a, int b)
                {
                    var t = a;
                    a = b;
                    b = t;
                    System.Console.WriteLine(t);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_tuple_swap"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NotASwapPattern_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int a, int b)
                {
                    var t = a;
                    a = b;
                    b = a;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_tuple_swap"] = "true",
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
                void M(int a, int b)
                {
                    var t = a;
                    a = b;
                    b = t;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_tuple_swap"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
