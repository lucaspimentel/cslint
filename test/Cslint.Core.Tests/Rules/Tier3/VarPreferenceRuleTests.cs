using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class VarPreferenceRuleTests
{
    private readonly VarPreferenceRule _rule = new();

    [Fact]
    public void Analyze_VarWithApparentType_WhenPreferred_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { var x = new List<int>(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExplicitTypeWhenApparent_WhenVarPreferred_ReturnsDiagnostic()
    {
        string source = "class C { void M() { List<int> x = new List<int>(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT200", diagnostics[0].RuleId);
    }

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
    }
}
