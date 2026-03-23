using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ColonSpacingRuleTests
{
    private readonly ColonSpacingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_colon_spacing"] = "true",
        });

    [Fact]
    public void Analyze_NoSpaceAroundBaseListColon_ReturnsDiagnostics()
    {
        string source = "class C:System.Object { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.NotEmpty(diagnostics);
    }

    [Fact]
    public void Analyze_CorrectBaseListColonSpacing_ReturnsNoDiagnostics()
    {
        string source = "class C : System.Object { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C:System.Object { }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_colon_spacing"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("csharp_space_before_colon_in_inheritance_clause")]
    [InlineData("csharp_space_after_colon_in_inheritance_clause")]
    public void IsEnabled_StandardKey_ReturnsTrue(string key)
    {
        var config = new LintConfiguration(new Dictionary<string, string> { [key] = "true" });

        Assert.True(_rule.IsEnabled(config));
    }
}
