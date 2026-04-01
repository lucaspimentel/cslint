using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class VisibleNonConstStaticFieldRuleTests
{
    private readonly VisibleNonConstStaticFieldRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA2211.severity"] = "warning",
    });

    [Theory]
    [InlineData("class C { public static int Field; }")]
    [InlineData("class C { protected static int Field; }")]
    public void Analyze_VisibleMutableStaticField_ReturnsDiagnostic(
        string source)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA2211", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("class C { public const int Field = 0; }")]
    [InlineData("class C { public static readonly int Field = 0; }")]
    [InlineData("class C { private static int Field; }")]
    [InlineData("class C { internal static int Field; }")]
    [InlineData("class C { public int Field; }")]
    public void Analyze_ConstOrReadonlyOrNonVisible_NoDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "class C { public static int Field; }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2211.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
