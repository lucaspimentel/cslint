using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PreferIsNullRuleTests
{
    private readonly PreferIsNullRule _rule = new();

    private static LintConfiguration Enabled =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_is_null_check_over_reference_equality_method"] = "true",
        });

    [Fact]
    public void Analyze_ReferenceEquals_NullSecond_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(object x) => ReferenceEquals(x, null);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0041", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ReferenceEquals_NullFirst_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(object x) => ReferenceEquals(null, x);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ObjectDotReferenceEquals_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(object x) => object.ReferenceEquals(x, null);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ObjectCapitalDotReferenceEquals_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(object x) => Object.ReferenceEquals(x, null);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ReferenceEquals_NoNull_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(object x, object y) => ReferenceEquals(x, y);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IsNull_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(object x) => x is null;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigNotSet_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(object x) => ReferenceEquals(x, null);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>());
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(object x) => ReferenceEquals(x, null);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_is_null_check_over_reference_equality_method"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
