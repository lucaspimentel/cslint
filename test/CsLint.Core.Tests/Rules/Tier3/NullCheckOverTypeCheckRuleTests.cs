using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NullCheckOverTypeCheckRuleTests
{
    private readonly NullCheckOverTypeCheckRule _rule = new();

    [Fact]
    public void Analyze_IsObject_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(object? x) => x is object;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0150", diagnostics[0].RuleId);
        Assert.Contains("is not null", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_IsNotObject_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                bool M(object? x) => x is not object;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("is null", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_IsObjectInIfCondition_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(object? x)
                {
                    if (x is object)
                    {
                        System.Console.WriteLine(x);
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_IsObjectWithDeclaration_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object? x)
                {
                    if (x is object o)
                    {
                        System.Console.WriteLine(o);
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Declaration pattern (x is object o) — value is used, don't flag
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IsString_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(object? x) => x is string;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Not 'object' — can't determine equivalence without semantic analysis
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IsNotNull_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(object? x) => x is not null;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("FALSE")]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics(string value)
    {
        string source = """
            class C
            {
                bool M(object? x) => x is object;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = value,
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                bool M(object? x) => x is object;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IsNotObjectInGuardClause_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(object? x)
                {
                    if (x is not object)
                        return;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_MultiplePatterns_ReportsAll()
    {
        string source = """
            class C
            {
                void M(object? x, object? y)
                {
                    bool a = x is object;
                    bool b = y is not object;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_null_check_over_type_check"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }
}
