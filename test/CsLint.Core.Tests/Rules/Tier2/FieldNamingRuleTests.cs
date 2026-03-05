using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class FieldNamingRuleTests
{
    private readonly FieldNamingRule _rule = new();

    [Theory]
    [InlineData("class C { private int _count; }")]
    [InlineData("class C { int _value; }")] // implicitly private
    [InlineData("class C { private readonly int _items; }")]
    public void Analyze_UnderscoreCamelCase_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { private int count; }", "count")]
    [InlineData("class C { private int Count; }", "Count")]
    [InlineData("class C { int value; }", "value")]
    public void Analyze_NonUnderscoreCamelCase_ReturnsDiagnostics(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT104", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
    }

    [Theory]
    [InlineData("class C { public int Count; }")]           // public
    [InlineData("class C { protected int Count; }")]        // protected
    [InlineData("class C { private const int Max = 10; }")] // const
    [InlineData("class C { private static int Count; }")]   // static
    public void Analyze_SkipsNonPrivateInstanceFields(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
