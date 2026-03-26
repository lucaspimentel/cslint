using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EnumValuesPrefixedWithTypeNameRuleTests
{
    private readonly EnumValuesPrefixedWithTypeNameRule _rule = new();

    [Fact]
    public void Analyze_AllValuesPrefixed_ReturnsDiagnostic()
    {
        const string source = """
            enum Color
            {
                ColorRed,
                ColorGreen,
                ColorBlue,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1712", diagnostic.RuleId);
        Assert.Contains("Color", diagnostic.Message);
    }

    [Fact]
    public void Analyze_NoValuesPrefixed_NoDiagnostic()
    {
        const string source = """
            enum Color
            {
                Red,
                Green,
                Blue,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SomeValuesPrefixed_BelowThreshold_NoDiagnostic()
    {
        const string source = """
            enum Color
            {
                ColorRed,
                Green,
                Blue,
                Yellow,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SingleMember_NoDiagnostic()
    {
        const string source = "enum Color { ColorRed }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            enum Color { ColorRed, ColorGreen, ColorBlue }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1712.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
