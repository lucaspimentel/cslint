using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class LocalVariableNamingRuleTests
{
    private readonly LocalVariableNamingRule _rule = new();

    [Theory]
    [InlineData("class C { void M() { int count = 0; } }")]
    [InlineData("class C { void M() { int @case = 0; } }")] // verbatim
    [InlineData("class C { void M() { foreach (var item in new int[0]) { } } }")]
    [InlineData("class C { void M() { foreach (var @item in new int[0]) { } } }")] // verbatim foreach
    [InlineData("class C { void M() { int _ = 0; } }")] // discard
    public void Analyze_CamelCaseOrDiscards_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("class C { void M() { int Count = 0; } }", "Count")]
    [InlineData("class C { void M() { int _count = 0; } }", "_count")]
    public void Analyze_NonCamelCase_ReturnsDiagnostic(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1312", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
        Assert.Contains("local variable", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ForEachNonCamelCase_ReturnsDiagnostic()
    {
        string source = "class C { void M() { foreach (var Item in new int[0]) { } } }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1312", diagnostics[0].RuleId);
        Assert.Contains("Item", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_LocalConst_IsSkipped()
    {
        string source = "class C { void M() { const int MaxValue = 10; } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
