using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class ConstantNamingRuleTests
{
    private readonly ConstantNamingRule _rule = new();

    [Theory]
    [InlineData("class C { const int MaxValue = 10; }")]
    [InlineData("class C { const int MAX_VALUE = 10; }")]
    [InlineData("class C { void M() { const int MaxValue = 10; } }")]
    public void Analyze_ValidConstantNames_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { const int maxValue = 10; }", "maxValue")]
    [InlineData("class C { const int _value = 10; }", "_value")]
    [InlineData("class C { void M() { const int camelCase = 1; } }", "camelCase")]
    public void Analyze_InvalidConstantNames_ReturnsDiagnostics(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT105", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
    }
}
