using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NoBlankLineAfterOpeningBraceRuleTests
{
    private readonly NoBlankLineAfterOpeningBraceRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_blank_line_after_opening_brace"] = "true",
        });

    [Fact]
    public void Analyze_BlankLineAfterClassBrace_ReturnsDiagnostic()
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
        Assert.Equal("CSLINT247", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BlankLineAfterMethodBrace_ReturnsDiagnostic()
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
        Assert.Equal("CSLINT247", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NoBlankLineAfterBrace_ReturnsNoDiagnostics()
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
        string source = "class C\n{\n\n    void M() { }\n}";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_blank_line_after_opening_brace"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
