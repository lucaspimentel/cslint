using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SemicolonSpacingRuleTests
{
    private readonly SemicolonSpacingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_semicolon_spacing"] = "true",
        });

    [Fact]
    public void Analyze_SpaceBeforeSemicolon_ReturnsDiagnostic()
    {
        string source = "class C { void M() { int x = 1 ; } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT256", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NoSpaceBeforeSemicolon_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { int x = 1; } }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { void M() { int x = 1 ; } }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_semicolon_spacing"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
