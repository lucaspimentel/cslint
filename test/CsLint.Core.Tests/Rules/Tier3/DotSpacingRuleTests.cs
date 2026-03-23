using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class DotSpacingRuleTests
{
    private readonly DotSpacingRule _rule = new();

    [Fact]
    public void Analyze_SpaceAfterDot_WhenNotRequired_ReturnsDiagnostic()
    {
        string source = "class C { void M() { System. Console.WriteLine(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_after_dot"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.NotEmpty(_rule.Analyze(context));
        Assert.Equal("CSLINT289", _rule.Analyze(context)[0].RuleId);
    }

    [Fact]
    public void Analyze_NoSpaceAroundDot_WhenNotRequired_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { System.Console.WriteLine(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_after_dot"] = "false",
            ["csharp_space_before_dot"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { System. Console.WriteLine(); } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
