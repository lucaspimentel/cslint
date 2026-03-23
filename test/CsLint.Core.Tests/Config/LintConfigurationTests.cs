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

    [Theory]
    [InlineData("true:error", true)]
    [InlineData("true:warning", true)]
    [InlineData("true:suggestion", true)]
    [InlineData("false:error", false)]
    [InlineData("false:warning", false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void GetBool_WithAndWithoutSeveritySuffix_ReturnsCorrectValue(string raw, bool expected)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["key"] = raw });

        Assert.Equal(expected, config.GetBool("key"));
    }

    [Theory]
    [InlineData("120:warning", 120)]
    [InlineData("80:error", 80)]
    [InlineData("120", 120)]
    public void GetInt_WithAndWithoutSeveritySuffix_ReturnsCorrectValue(string raw, int expected)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["key"] = raw });

        Assert.Equal(expected, config.GetInt("key"));
    }

    [Theory]
    [InlineData("true:error", "true", "error")]
    [InlineData("false:warning", "false", "warning")]
    [InlineData("true", "true", null)]
    [InlineData("when_on_single_line:silent", "when_on_single_line", "silent")]
    public void GetValueWithSeverity_SplitsCorrectly(string raw, string expectedValue, string? expectedSeverity)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["key"] = raw });

        (string? value, string? severity) = config.GetValueWithSeverity("key");

        Assert.Equal(expectedValue, value);
        Assert.Equal(expectedSeverity, severity);
    }

    [Fact]
    public void GetFirstValue_ReturnsFirstMatchingKey()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["key_b"] = "value_b" });

        string? result = config.GetFirstValue("key_a", "key_b");

        Assert.Equal("value_b", result);
    }

    [Fact]
    public void GetFirstValue_PrefersFirstKey()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["key_a"] = "value_a",
                ["key_b"] = "value_b",
            });

        string? result = config.GetFirstValue("key_a", "key_b");

        Assert.Equal("value_a", result);
    }

    [Fact]
    public void GetFirstValue_NoMatch_ReturnsNull()
    {
        string? result = LintConfiguration.Empty.GetFirstValue("key_a", "key_b");

        Assert.Null(result);
    }
}
