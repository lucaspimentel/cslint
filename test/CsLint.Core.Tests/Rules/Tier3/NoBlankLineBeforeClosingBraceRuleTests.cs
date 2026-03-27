using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NoBlankLineBeforeClosingBraceRuleTests
{
    private readonly NoBlankLineBeforeClosingBraceRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_blank_line_before_closing_brace"] = "true",
        });

    [Fact]
    public void Analyze_BlankLineBeforeClassClosingBrace_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M() { }

            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1508", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BlankLineBeforeMethodClosingBrace_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;

                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1508", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NoBlankLineBeforeClosingBrace_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EmptyBlock_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C\n{\n    void M() { }\n\n}";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_blank_line_before_closing_brace"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
