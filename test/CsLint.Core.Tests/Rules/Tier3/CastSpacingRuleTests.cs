using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class CastSpacingRuleTests
{
    private readonly CastSpacingRule _rule = new();

    [Fact]
    public void Analyze_NoSpaceAfterCast_WhenRequired_ReturnsDiagnostic()
    {
        string source = "class C { void M() { var x = (int)1.5; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_after_cast"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT286", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SpaceAfterCast_WhenRequired_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { var x = (int) 1.5; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_after_cast"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SpaceAfterCast_WhenNotRequired_ReturnsDiagnostic()
    {
        string source = "class C { void M() { var x = (int) 1.5; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_after_cast"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { var x = (int)1.5; } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
