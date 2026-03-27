using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ElementsSeparatedByBlankLineRuleTests
{
    private readonly ElementsSeparatedByBlankLineRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_elements_separated_by_blank_line"] = "true",
        });

    [Fact]
    public void Analyze_MethodsWithoutBlankLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M1() { }
                void M2() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1516", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MethodAndPropertyWithoutBlankLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int X { get; }
                void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ThreeMethodsWithoutBlankLines_ReturnsTwoDiagnostics()
    {
        string source = """
            class C
            {
                void M1() { }
                void M2() { }
                void M3() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void Analyze_MethodsWithBlankLine_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M1() { }

                void M2() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleMember_ReturnsNoDiagnostics()
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
        string source = """
            class C
            {
                void M1() { }
                void M2() { }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_elements_separated_by_blank_line"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
