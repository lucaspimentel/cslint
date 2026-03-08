using Cslint.Core.Config;
using Cslint.Core.Rules;

namespace Cslint.Core.Tests.Config;

public class LintConfigurationTests
{
    [Theory]
    [InlineData("error", LintSeverity.Error)]
    [InlineData("Error", LintSeverity.Error)]
    [InlineData("warning", LintSeverity.Warning)]
    [InlineData("Warning", LintSeverity.Warning)]
    [InlineData("suggestion", LintSeverity.Info)]
    [InlineData("Suggestion", LintSeverity.Info)]
    [InlineData("silent", LintSeverity.None)]
    [InlineData("Silent", LintSeverity.None)]
    [InlineData("none", LintSeverity.None)]
    [InlineData("None", LintSeverity.None)]
    public void ParseSeverity_ValidValues_ReturnsExpectedSeverity(string input, LintSeverity expected)
    {
        LintSeverity? result = LintConfiguration.ParseSeverity(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("info")]
    public void ParseSeverity_InvalidOrNull_ReturnsNull(string? input)
    {
        LintSeverity? result = LintConfiguration.ParseSeverity(input);
        Assert.Null(result);
    }

    [Fact]
    public void GetSeverityForKey_WithSeveritySuffix_ReturnsParsedSeverity()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["csharp_style_var_elsewhere"] = "true:error" });

        LintSeverity? severity = config.GetSeverityForKey("csharp_style_var_elsewhere");

        Assert.Equal(LintSeverity.Error, severity);
    }

    [Fact]
    public void GetSeverityForKey_WithoutSeveritySuffix_ReturnsNull()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["csharp_style_var_elsewhere"] = "true" });

        LintSeverity? severity = config.GetSeverityForKey("csharp_style_var_elsewhere");

        Assert.Null(severity);
    }

    [Fact]
    public void GetSeverityForKey_MissingKey_ReturnsNull()
    {
        LintSeverity? severity = LintConfiguration.Empty.GetSeverityForKey("nonexistent_key");
        Assert.Null(severity);
    }
}
