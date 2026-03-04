using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Tests.Rules.Tier1;

public class TrailingWhitespaceRuleTests
{
    private readonly TrailingWhitespaceRule _rule = new();

    [Theory]
    [InlineData("class Foo { }")]
    [InlineData("class Foo { }\n")]
    [InlineData("int x = 1;\nint y = 2;\n")]
    public void Analyze_NoTrailingWhitespace_ReturnsNoLintDiagnostics(string source)
    {
        RuleContext context = CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TrailingSpaces_ReturnsLintDiagnostic()
    {
        string source = "class Foo { }   \n";
        RuleContext context = CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT001", diagnostics[0].RuleId);
        Assert.Equal(1, diagnostics[0].Line);
    }

    [Fact]
    public void Analyze_TrailingTabs_ReturnsLintDiagnostic()
    {
        string source = "class Foo { }\t\n";
        RuleContext context = CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Theory]
    [InlineData("line1  \nline2\nline3   \n", 2)]
    [InlineData("a \nb \nc \n", 3)]
    public void Analyze_MultipleViolations_ReturnsCorrectCount(string source, int expectedCount)
    {
        RuleContext context = CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(expectedCount, diagnostics.Count);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsEnabled_RespectsConfiguration(bool configValue, bool expected)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = configValue.ToString().ToLowerInvariant() });

        Assert.Equal(expected, _rule.IsEnabled(config));
    }

    private static RuleContext CreateContext(string source, string filePath = "test.cs")
    {
        SourceText sourceText = SourceText.From(source);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);

        return new RuleContext
        {
            FilePath = filePath,
            SourceText = sourceText,
            SyntaxTree = tree,
            Root = (CSharpSyntaxNode)tree.GetRoot(),
            Configuration = LintConfiguration.Empty,
        };
    }
}
