using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SimplifiedBooleanExpressionRuleTests
{
    private readonly SimplifiedBooleanExpressionRule _rule = new();

    [Fact]
    public void Analyze_CondTrueFalse_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(bool cond) => cond ? true : false;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_boolean_expressions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT235", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_CondFalseTrue_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(bool cond) => cond ? false : true;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_boolean_expressions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT235", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_CondTrueExpr_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(bool cond, bool expr) => cond ? true : expr;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_boolean_expressions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT235", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_CondExprFalse_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(bool cond, bool expr) => cond ? expr : false;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_boolean_expressions"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT235", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NoBooleanLiterals_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(bool cond, bool x, bool y) => cond ? x : y;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_boolean_expressions"] = "true",
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
                bool M(bool cond) => cond ? true : false;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_simplified_boolean_expressions"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(bool cond) => cond ? true : false;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
