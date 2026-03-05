using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class MaxLineLengthRuleTests
{
    private readonly MaxLineLengthRule _rule = new();

    [Theory]
    [InlineData("short line", "80")]
    [InlineData("12345678901234567890", "20")]
    public void Analyze_WithinLimit_ReturnsNoDiagnostics(string source, string maxLength)
    {
        RuleContext context = CreateContext(source, maxLength);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExceedsLimit_ReturnsDiagnostic()
    {
        string source = new string('x', 81);
        RuleContext context = CreateContext(source, "80");

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT005", diagnostics[0].RuleId);
        Assert.Contains("81", diagnostics[0].Message);
        Assert.Contains("80", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("off", false)]
    [InlineData("80", true)]
    [InlineData(null, false)]
    public void IsEnabled_RespectsConfiguration(string? value, bool expected)
    {
        var props = new Dictionary<string, string>();

        if (value is not null)
        {
            props["max_line_length"] = value;
        }

        var config = new LintConfiguration(props);

        Assert.Equal(expected, _rule.IsEnabled(config));
    }

    private static RuleContext CreateContext(string source, string maxLength)
    {
        var config = new LintConfiguration(new Dictionary<string, string> { ["max_line_length"] = maxLength });
        return TestHelper.CreateContext(source, config);
    }
}
