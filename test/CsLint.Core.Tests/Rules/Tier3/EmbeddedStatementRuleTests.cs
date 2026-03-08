using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EmbeddedStatementRuleTests
{
    private readonly EmbeddedStatementRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_allow_embedded_statements_on_same_line"] = "false",
        });

    [Theory]
    [InlineData("class C { void M() { if (true) return; } }")]
    [InlineData("class C { void M() { while (true) return; } }")]
    [InlineData("class C { void M() { for (;;) return; } }")]
    [InlineData("class C { void M() { foreach (var x in new[]{1}) return; } }")]
    public void Analyze_EmbeddedOnSameLine_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT228", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("class C { void M() { if (true)\n    return; } }")]
    [InlineData("class C { void M() { while (true)\n    return; } }")]
    [InlineData("class C { void M() { for (;;)\n    return; } }")]
    public void Analyze_EmbeddedOnNextLine_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { void M() { if (true) { return; } } }")]
    [InlineData("class C { void M() { while (true) { return; } } }")]
    public void Analyze_WithBlock_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ElseClauseOnSameLine_ReturnsDiagnostic()
    {
        string source = "class C { void M() { if (true) { } else return; } }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT228", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_DoStatement_ReturnsDiagnostic()
    {
        string source = "class C { void M() { do return; while (true); } }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT228", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_UsingStatement_ReturnsDiagnostic()
    {
        string source = "class C { void M() { using (var x = new object()) return; } }";
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT228", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ConfigAllowed_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { if (true) return; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_allow_embedded_statements_on_same_line"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { if (true) return; } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
