using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class ParameterLocalNamingRuleTests
{
    private readonly ParameterLocalNamingRule _rule = new();

    [Theory]
    [InlineData("class C { void M(int value) { } }")]
    [InlineData("class C { void M() { int count = 0; } }")]
    [InlineData("class C { void M(int _) { } }")] // discard
    [InlineData("class C { void M(object @event) { } }")] // verbatim parameter
    [InlineData("class C { void M() { int @case = 0; } }")] // verbatim local variable
    [InlineData("record C(string Name, int Value);")] // record positional parameters
    [InlineData("class C { void M() { foreach (var @item in new int[0]) { } } }")] // verbatim foreach variable
    public void Analyze_CamelCaseNamesOrDiscards_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { void M(int Value) { } }", "parameter", "Value")]
    [InlineData("class C { void M() { int Count = 0; } }", "local variable", "Count")]
    public void Analyze_NonCamelCase_ReturnsDiagnostics(string source, string kind, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT103", diagnostics[0].RuleId);
        Assert.Contains(kind, diagnostics[0].Message);
        Assert.Contains(name, diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_LocalConst_IsSkipped()
    {
        string source = "class C { void M() { const int MaxValue = 10; } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
