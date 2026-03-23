using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SquareBracketSpacingRuleTests
{
    private readonly SquareBracketSpacingRule _rule = new();

    [Fact]
    public void Analyze_SpaceBeforeOpenBracket_WhenNotRequired_ReturnsDiagnostic()
    {
        string source = "class C { void M() { var x = new int [3]; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_before_open_square_brackets"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.NotEmpty(_rule.Analyze(context));
        Assert.Equal("CSLINT290", _rule.Analyze(context)[0].RuleId);
    }

    [Fact]
    public void Analyze_NoSpaceBeforeOpenBracket_WhenNotRequired_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { var x = new int[3]; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_before_open_square_brackets"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { var x = new int [3]; } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
