using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NullCheckingRuleTests
{
    private readonly NullCheckingRule _rule = new();

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
        Assert.Equal("CSLINT210", diagnostics[0].RuleId);
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

    [Theory]
    [InlineData(
        "if (s == null) { throw new System.ArgumentNullException(); } _s = s;",
        "assignment follows")]
    [InlineData(
        "if (s == null) { throw new System.ArgumentNullException(); } return s;",
        "return follows")]
    [InlineData(
        "if (null == s) { throw new System.ArgumentNullException(); } _s = s;",
        "reversed null check with assignment")]
    public void Analyze_NullCheckIfThrow_WhenNextStatementUsesIdentifier_ReturnsDiagnostic(string statements, string _)
    {
        string source = $$"""
            class C {
                string _s;
                string M(string s) {
                    {{statements}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT210", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData(
        "if (s == null) { throw new System.ArgumentNullException(); }",
        "standalone guard, no next statement")]
    [InlineData(
        "if (s == null) { throw new System.ArgumentNullException(); } DoSomething(s);",
        "next statement is method call")]
    [InlineData(
        "if (s == null) { throw new System.ArgumentNullException(); } _s = other;",
        "assignment of different variable")]
    [InlineData(
        "if (s == null) { throw new System.ArgumentNullException(); } _s = s.Length;",
        "RHS is member access, not bare identifier")]
    public void Analyze_NullCheckIfThrow_WhenNextStatementDoesNotUseIdentifier_ReturnsNoDiagnostics(string statements, string _)
    {
        string source = $$"""
            class C {
                string _s;
                string other;
                void M(string s) {
                    {{statements}}
                }
                void DoSomething(string s) {}
            }
            """;
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
    public void IsEnabled_CsLintKeyTrue_ReturnsTrue()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["dotnet_style_null_checking"] = "true" });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_CsLintKeyFalse_ReturnsFalse()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["dotnet_style_null_checking"] = "false" });

        Assert.False(_rule.IsEnabled(config));
    }

    [Theory]
    [InlineData("dotnet_style_coalesce_expression")]
    [InlineData("dotnet_style_null_propagation")]
    [InlineData("dotnet_style_prefer_is_null_check_over_reference_equality_method")]
    public void IsEnabled_StandardKeyTrue_ReturnsTrue(string key)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { [key] = "true:warning" });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_AllStandardKeysFalse_ReturnsFalse()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["dotnet_style_coalesce_expression"] = "false:none",
                ["dotnet_style_null_propagation"] = "false:none",
                ["dotnet_style_prefer_is_null_check_over_reference_equality_method"] = "false:none",
            });

        Assert.False(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_CsLintKeyTakesPrecedence()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["dotnet_style_null_checking"] = "false",
                ["dotnet_style_coalesce_expression"] = "true:warning",
            });

        // CsLint key says disabled, should take precedence
        Assert.False(_rule.IsEnabled(config));
    }
}
