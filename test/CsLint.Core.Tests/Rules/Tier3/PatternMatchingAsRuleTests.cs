using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PatternMatchingAsRuleTests
{
    private readonly PatternMatchingAsRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_pattern_matching_over_as_with_null_check"] = "true:warning",
        });

    [Theory]
    [InlineData("x != null")]
    [InlineData("null != x")]
    public void Analyze_AsWithNullCheck_ReturnsDiagnostic(string nullCheck)
    {
        string source = $$"""
            class C
            {
                void M(object o)
                {
                    var x = o as string;
                    if ({{nullCheck}})
                    {
                        System.Console.WriteLine(x);
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT270", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_AsWithIsNotNull_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    var x = o as string;
                    if (x is not null)
                    {
                        System.Console.WriteLine(x);
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_IsPatternMatching_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    if (o is string x)
                    {
                        System.Console.WriteLine(x);
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AsWithoutNullCheck_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    var x = o as string;
                    System.Console.WriteLine(x);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AsNullCheckDifferentVariable_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object o, string y)
                {
                    var x = o as string;
                    if (y != null)
                    {
                        System.Console.WriteLine(x);
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigDisabled_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    var x = o as string;
                    if (x != null) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_pattern_matching_over_as_with_null_check"] = "false:warning",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object o)
                {
                    var x = o as string;
                    if (x != null) { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
