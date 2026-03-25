using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NullCoalescingRuleTests
{
    private readonly NullCoalescingRule _rule = new();

    [Fact]
    public void Analyze_CoalesceOperator_ReturnsNoDiagnostics()
    {
        string source = "class C { void M(string s) { var x = s ?? \"\"; } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("s != null ? s : \"\"", "variable on right of !=")]
    [InlineData("null != s ? s : \"\"", "variable on left of !=")]
    public void Analyze_NullCheckConditional_WhenTrueBranchIsCheckedVariable_ReturnsDiagnostic(string ternary, string _)
    {
        string source = $"class C {{ void M(string s) {{ var x = {ternary}; }} }}";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0029", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("x != null ? x.GetHashCode() : 0", "method call on variable")]
    [InlineData("x != null ? x.ToString() : \"none\"", "method call returning different type")]
    [InlineData("x != null ? x.Length : 0", "member access")]
    [InlineData("x != null ? other : \"\"", "different variable in true branch")]
    public void Analyze_NullCheckConditional_WhenTrueBranchIsNotCheckedVariable_ReturnsNoDiagnostics(string ternary, string _)
    {
        string source = $"class C {{ void M(string x, string other) {{ var r = {ternary}; }} }}";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void IsEnabled_NoKeysPresent_ReturnsTrue()
    {
        var config = new LintConfiguration(new Dictionary<string, string>());

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_OverrideKeyFalse_ReturnsFalse()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["dotnet_style_null_checking"] = "false" });

        Assert.False(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_ConfigKeyTrue_ReturnsTrue()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["dotnet_style_coalesce_expression"] = "true:warning" });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_ConfigKeyFalse_ReturnsFalse()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["dotnet_style_coalesce_expression"] = "false:none" });

        Assert.False(_rule.IsEnabled(config));
    }
}
