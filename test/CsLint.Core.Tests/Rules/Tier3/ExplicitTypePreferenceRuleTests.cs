using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExplicitTypePreferenceRuleTests
{
    private readonly ExplicitTypePreferenceRule _rule = new();

    [Fact]
    public void Analyze_VarForBuiltIn_WhenExplicitPreferred_ReturnsDiagnostic()
    {
        string source = "class C { void M() { var x = 42; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_for_built_in_types"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0008", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_VarElsewhere_WhenExplicitPreferred_ReturnsDiagnostic()
    {
        string source = "class C { void M() { var x = GetValue(); } int GetValue() => 1; }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_elsewhere"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0008", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_VarWithApparentType_WhenExplicitPreferred_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { var x = new List<int>(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_elsewhere"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExplicitType_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { int x = 42; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_for_built_in_types"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
