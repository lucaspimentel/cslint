using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class PrivateFieldNamingRuleTests
{
    private readonly PrivateFieldNamingRule _rule = new();

    [Theory]
    [InlineData("class C { private int _count; }")]
    [InlineData("class C { int _value; }")] // implicitly private
    [InlineData("class C { private readonly int _items; }")]
    [InlineData("class C { private int @_count; }")] // verbatim
    public void Analyze_UnderscoreCamelCase_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("class C { private int count; }", "count")]
    [InlineData("class C { private int Count; }", "Count")]
    [InlineData("class C { int value; }", "value")] // implicitly private
    public void Analyze_NonUnderscoreCamelCase_ReturnsDiagnostic(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1306", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
    }

    [Theory]
    [InlineData("class C { public int Count; }")]           // non-private
    [InlineData("class C { private const int Max = 10; }")] // const
    [InlineData("class C { private static int Count; }")]   // static
    public void Analyze_SkipsNonMatchingFields(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("[StructLayout(LayoutKind.Sequential)] struct S { int dwLength; }")]
    [InlineData("[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)] struct S { int dwLength; }")]
    public void Analyze_StructLayoutFields_AreIgnored(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
