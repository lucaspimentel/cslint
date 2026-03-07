using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SimplifyInterpolationRuleTests
{
    private readonly SimplifyInterpolationRule _rule = new();

    [Fact]
    public void Analyze_ToStringInInterpolation_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    var s = $"{x.ToString()}";
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_interpolation"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT225", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_AlreadySimplified_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    var s = $"{x}";
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_interpolation"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ToStringWithFormat_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    var s = $"{x.ToString("N2")}";
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_interpolation"] = "true",
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
                void M(int x)
                {
                    var s = $"{x.ToString()}";
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_interpolation"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
