using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class TrailingCommasRuleTests
{
    private readonly TrailingCommasRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_trailing_commas_in_multi_line_initializers"] = "true",
        });

    [Fact]
    public void Analyze_MultiLineEnumWithoutTrailingComma_ReturnsDiagnostic()
    {
        string source = """
            enum Color
            {
                Red,
                Green,
                Blue
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1413", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultiLineEnumWithTrailingComma_ReturnsNoDiagnostics()
    {
        string source = """
            enum Color
            {
                Red,
                Green,
                Blue,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleLineEnum_ReturnsNoDiagnostics()
    {
        string source = "enum Color { Red, Green, Blue }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultiLineInitializerWithoutTrailingComma_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int[] x = new[]
                {
                    1,
                    2,
                    3
                };
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1413", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultiLineInitializerWithTrailingComma_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                int[] x = new[]
                {
                    1,
                    2,
                    3,
                };
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleLineInitializer_ReturnsNoDiagnostics()
    {
        string source = "class C { int[] x = new[] { 1, 2, 3 }; }";
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
            enum Color
            {
                Red,
                Green,
                Blue
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_trailing_commas_in_multi_line_initializers"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
