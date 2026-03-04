using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class MemberNamingRuleTests
{
    private readonly MemberNamingRule _rule = new();

    [Theory]
    [InlineData("class C { void DoWork() { } }")]
    [InlineData("class C { int Count { get; } }")]
    [InlineData("class C { event EventHandler Changed; }")]
    public void Analyze_PascalCaseMembers_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { void doWork() { } }", "method", "doWork")]
    [InlineData("class C { int count { get; } }", "property", "count")]
    public void Analyze_NonPascalCaseMembers_ReturnsDiagnostics(string source, string kind, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT102", diagnostics[0].RuleId);
        Assert.Contains(kind, diagnostics[0].Message);
        Assert.Contains(name, diagnostics[0].Message);
    }
}
