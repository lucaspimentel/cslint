using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class ParameterNamingRuleTests
{
    private readonly ParameterNamingRule _rule = new();

    [Theory]
    [InlineData("class C { void M(int value) { } }")]
    [InlineData("class C { void M(int _) { } }")] // discard
    [InlineData("class C { void M(object @event) { } }")] // verbatim
    [InlineData("record C(string Name, int Value);")] // record positional parameters
    public void Analyze_CamelCaseOrSpecialCases_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("class C { void M(int Value) { } }", "Value")]
    [InlineData("class C { void M(int _value) { } }", "_value")]
    public void Analyze_NonCamelCase_ReturnsDiagnostic(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1313", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
        Assert.Contains("parameter", diagnostics[0].Message);
    }
}
