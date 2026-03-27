using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SingleStatementPerLineRuleTests
{
    private readonly SingleStatementPerLineRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_single_statement_per_line"] = "true",
        });

    [Fact]
    public void Analyze_TwoStatementsOnSameLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1; int y = 2;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1107", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ThreeStatementsOnSameLine_ReturnsTwoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1; int y = 2; int z = 3;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void Analyze_StatementsOnSeparateLines_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    int y = 2;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleStatement_ReturnsNoDiagnostics()
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

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1; int y = 2;
                }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_single_statement_per_line"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
