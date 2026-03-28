using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class StaticReadonlyFieldNamingRuleTests
{
    private readonly StaticReadonlyFieldNamingRule _rule = new();

    [Theory]
    [InlineData("class C { public static readonly int MaxValue; }")]
    [InlineData("class C { internal static readonly int Count; }")]
    [InlineData("class C { protected static readonly int Value; }")]
    public void Analyze_PascalCase_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("class C { public static readonly int count; }", "count")]
    [InlineData("class C { internal static readonly int maxValue; }", "maxValue")]
    public void Analyze_NonPascalCase_ReturnsDiagnostic(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1311", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
    }

    [Theory]
    [InlineData("class C { private static readonly int _count; }")]    // private
    [InlineData("class C { public readonly int Count; }")]             // not static → SA1304
    [InlineData("class C { public static int Count; }")]               // not readonly → SA1307
    public void Analyze_SkipsNonMatchingFields(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
