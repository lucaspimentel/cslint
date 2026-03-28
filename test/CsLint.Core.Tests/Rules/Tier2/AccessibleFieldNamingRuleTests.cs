using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class AccessibleFieldNamingRuleTests
{
    private readonly AccessibleFieldNamingRule _rule = new();

    [Theory]
    [InlineData("class C { internal static int Count; }")]
    [InlineData("class C { public static int MaxValue; }")]
    public void Analyze_PascalCase_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("class C { internal static int count; }", "count")]
    [InlineData("class C { public static int maxValue; }", "maxValue")]
    public void Analyze_NonPascalCase_ReturnsDiagnostic(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1307", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
    }

    [Theory]
    [InlineData("class C { private static int count; }")]              // private
    [InlineData("class C { public static readonly int Count; }")]      // static readonly → SA1311
    [InlineData("class C { public int Count; }")]                      // not static
    public void Analyze_SkipsNonMatchingFields(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
