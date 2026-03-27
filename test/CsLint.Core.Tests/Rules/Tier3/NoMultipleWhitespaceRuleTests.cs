using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NoMultipleWhitespaceRuleTests
{
    private readonly NoMultipleWhitespaceRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_multiple_whitespace"] = "true",
        });

    [Fact]
    public void Analyze_MultipleSpaces_ReturnsDiagnostic()
    {
        string source = "class C { int  x; }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.NotEmpty(diagnostics);
        Assert.Equal("SA1025", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SingleSpaces_ReturnsNoDiagnostics()
    {
        string source = "class C { int x; }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { int  x; }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_multiple_whitespace"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
