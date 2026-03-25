using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ThrowExpressionRuleTests
{
    private readonly ThrowExpressionRule _rule = new();

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
        Assert.Equal("IDE0016", diagnostics[0].RuleId);
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
            new Dictionary<string, string> { ["csharp_style_throw_expression"] = "true:warning" });

        Assert.True(_rule.IsEnabled(config));
    }
}
