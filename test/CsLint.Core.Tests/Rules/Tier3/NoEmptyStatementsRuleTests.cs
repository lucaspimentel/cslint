using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NoEmptyStatementsRuleTests
{
    private readonly NoEmptyStatementsRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_empty_statements"] = "true",
        });

    [Theory]
    [InlineData("void M() { ; }")]
    [InlineData("void M() { int x = 1; ; }")]
    public void Analyze_EmptyStatement_ReturnsDiagnostic(string method)
    {
        string source = $"class C {{ {method} }}";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT240", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultipleEmptyStatements_ReturnsMultipleDiagnostics()
    {
        string source = "class C { void M() { ; ; } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    [Theory]
    [InlineData("void M() { int x = 1; }")]
    [InlineData("void M() { return; }")]
    [InlineData("void M() { for (int i = 0; i < 10; i++) { } }")]
    public void Analyze_NormalStatements_ReturnsNoDiagnostics(string method)
    {
        string source = $"class C {{ {method} }}";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { void M() { ; } }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_empty_statements"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
