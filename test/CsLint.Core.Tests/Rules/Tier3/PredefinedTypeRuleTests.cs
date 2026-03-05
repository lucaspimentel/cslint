using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PredefinedTypeRuleTests
{
    private readonly PredefinedTypeRule _rule = new();

    private static LintConfiguration PreferPredefined =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_predefined_type_for_locals_parameters_members"] = "true",
            ["dotnet_style_predefined_type_for_member_access"] = "true",
        });

    [Fact]
    public void Analyze_PredefinedTypes_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { int x = 0; string s = \"\"; } }";
        RuleContext context = TestHelper.CreateContext(source, PreferPredefined);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FrameworkMemberAccess_ReturnsDiagnostic()
    {
        string source = "class C { void M() { var x = Int32.MaxValue; } }";
        RuleContext context = TestHelper.CreateContext(source, PreferPredefined);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT208", diagnostics[0].RuleId);
        Assert.Contains("Int32", diagnostics[0].Message);
    }
}
