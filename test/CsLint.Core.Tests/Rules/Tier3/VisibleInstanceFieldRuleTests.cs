using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class VisibleInstanceFieldRuleTests
{
    private readonly VisibleInstanceFieldRule _rule = new();

    [Theory]
    [InlineData("class C { public int Field; }")]
    [InlineData("class C { protected int Field; }")]
    public void Analyze_VisibleInstanceField_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1051", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("class C { private int _field; }")]
    [InlineData("class C { internal int Field; }")]
    [InlineData("class C { public static int Field; }")]
    [InlineData("class C { public const int Field = 0; }")]
    [InlineData("struct S { public int Field; }")]
    [InlineData("readonly struct S { public readonly int Field; }")]
    [InlineData("record struct S(int X) { public int Field; }")]
    public void Analyze_NonVisibleOrStaticField_NoDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "class C { public int Field; }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1051.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
