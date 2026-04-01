using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class WriteOnlyPropertyRuleTests
{
    private readonly WriteOnlyPropertyRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1044.severity"] = "warning",
    });

    [Fact]
    public void Analyze_SetterOnly_ReturnsDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value { set { } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1044", diagnostic.RuleId);
        Assert.Contains("Value", diagnostic.Message);
    }

    [Fact]
    public void Analyze_InitOnly_ReturnsDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value { init { } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_GetterAndSetter_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_GetterOnly_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value { get; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExpressionBodied_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value => 42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "class C { public int Value { set { } } }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1044.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
