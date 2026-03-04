using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ThisQualificationRuleTests
{
    private readonly ThisQualificationRule _rule = new();

    private static LintConfiguration NoThisConfig =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_qualification_for_field"] = "false",
            ["dotnet_style_qualification_for_property"] = "false",
            ["dotnet_style_qualification_for_method"] = "false",
        });

    [Fact]
    public void Analyze_NoThisQualification_ReturnsNoDiagnostics()
    {
        string source = "class C { int _x; void M() { _x = 1; } }";
        RuleContext context = TestHelper.CreateContext(source, NoThisConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThisQualificationPresent_ReturnsDiagnostic()
    {
        string source = "class C { int _x; void M() { this._x = 1; } }";
        RuleContext context = TestHelper.CreateContext(source, NoThisConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT204", diagnostics[0].RuleId);
        Assert.Contains("this.", diagnostics[0].Message);
    }
}
