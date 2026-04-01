using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class IdentifierUnderscoreRuleTests
{
    private readonly IdentifierUnderscoreRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1707.severity"] = "warning",
    });

    [Theory]
    [InlineData("namespace N; class C { public void Do_Work() { } }")]
    [InlineData("namespace N; class C { public int My_Prop { get; } }")]
    [InlineData("namespace N; public class My_Class { }")]
    public void Analyze_UnderscoreInIdentifier_ReturnsDiagnostic(
        string source)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1707", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("namespace N; class C { private int _field; }")]
    [InlineData("namespace N; class C { private void Do_Work() { } }")]
    [InlineData("namespace N; class C { public void DoWork() { } }")]
    [InlineData("namespace N; enum E { Value_One }")]
    public void Analyze_PrivateOrNoUnderscore_NoDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_OverrideMethod_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public override string To_String() => "";
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "namespace N; class C { public void Do_Work() { } }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1707.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
