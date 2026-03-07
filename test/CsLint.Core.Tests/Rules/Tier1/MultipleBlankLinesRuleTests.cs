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
        Assert.Equal("CSLINT008", diagnostics[0].RuleId);
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
}
