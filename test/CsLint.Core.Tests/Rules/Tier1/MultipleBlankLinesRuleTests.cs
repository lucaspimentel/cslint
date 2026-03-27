using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class MultipleBlankLinesRuleTests
{
    private readonly MultipleBlankLinesRule _rule = new();

    [Theory]
    [InlineData("class Foo { }\n\nclass Bar { }\n")]
    [InlineData("class Foo { }\n")]
    [InlineData("")]
    public void Analyze_NoMultipleBlankLines_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TwoConsecutiveBlankLines_ReturnsOneDiagnostic()
    {
        string source = "class Foo { }\n\n\nclass Bar { }\n";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1507", diagnostics[0].RuleId);
        Assert.Equal(3, diagnostics[0].Line);
        Assert.Equal(1, diagnostics[0].Column);
    }

    [Fact]
    public void Analyze_ThreeConsecutiveBlankLines_ReturnsTwoDiagnostics()
    {
        string source = "class Foo { }\n\n\n\nclass Bar { }\n";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.Equal(3, diagnostics[0].Line);
        Assert.Equal(4, diagnostics[1].Line);
    }

    [Fact]
    public void Analyze_MultipleGroups_ReturnsCorrectCount()
    {
        string source = "a\n\n\nb\n\n\nc\n";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.Equal(3, diagnostics[0].Line);
        Assert.Equal(6, diagnostics[1].Line);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsEnabled_RespectsConfiguration(bool configValue, bool expected)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["csharp_no_multiple_blank_lines"] = configValue.ToString().ToLowerInvariant() });

        Assert.Equal(expected, _rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_MissingKey_ReturnsFalse()
    {
        var config = new LintConfiguration(new Dictionary<string, string>());

        Assert.False(_rule.IsEnabled(config));
    }

    [Theory]
    [InlineData("false:warning")]
    [InlineData("false:error")]
    [InlineData("false:suggestion")]
    public void IsEnabled_StandardKeyFalse_ReturnsTrue(string value)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["dotnet_style_allow_multiple_blank_lines_experimental"] = value,
            });

        Assert.True(_rule.IsEnabled(config));
    }

    [Theory]
    [InlineData("true:warning")]
    [InlineData("true:silent")]
    public void IsEnabled_StandardKeyTrue_ReturnsFalse(string value)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["dotnet_style_allow_multiple_blank_lines_experimental"] = value,
            });

        Assert.False(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_CsLintKeyTakesPrecedence()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["csharp_no_multiple_blank_lines"] = "false",
                ["dotnet_style_allow_multiple_blank_lines_experimental"] = "false:warning",
            });

        // CsLint key says disabled, should take precedence over standard key
        Assert.False(_rule.IsEnabled(config));
    }

    [Fact]
    public void Analyze_StandardKey_DetectsViolation()
    {
        string source = "class Foo { }\n\n\nclass Bar { }\n";
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["dotnet_style_allow_multiple_blank_lines_experimental"] = "false:warning",
            });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1507", diagnostics[0].RuleId);
    }
}
