using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class MethodCallSpacingRuleTests
{
    private readonly MethodCallSpacingRule _rule = new();

    [Fact]
    public void Analyze_SpaceBeforeParen_WhenNotRequired_ReturnsDiagnostic()
    {
        string source = "class C { void M() { System.Console.WriteLine (); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_between_method_call_name_and_opening_parenthesis"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
        Assert.Equal("CSLINT288", _rule.Analyze(context)[0].RuleId);
    }

    [Fact]
    public void Analyze_NoSpaceBeforeParen_WhenNotRequired_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { System.Console.WriteLine(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_between_method_call_name_and_opening_parenthesis"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { System.Console.WriteLine (); } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
