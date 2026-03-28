using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class ReadonlyFieldNamingRuleTests
{
    private readonly ReadonlyFieldNamingRule _rule = new();

    [Theory]
    [InlineData("class C { public readonly int Count; }")]
    [InlineData("class C { protected readonly int Value; }")]
    [InlineData("class C { internal readonly int Items; }")]
    public void Analyze_PascalCase_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("class C { public readonly int count; }", "count")]
    [InlineData("class C { protected readonly int myValue; }", "myValue")]
    public void Analyze_NonPascalCase_ReturnsDiagnostic(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1304", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
    }

    [Theory]
    [InlineData("class C { private readonly int _count; }")]        // private
    [InlineData("class C { public static readonly int Count; }")]   // static readonly → SA1311
    [InlineData("class C { public int Count; }")]                   // not readonly
    public void Analyze_SkipsNonMatchingFields(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
