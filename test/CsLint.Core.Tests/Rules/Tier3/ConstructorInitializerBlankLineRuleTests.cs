using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConstructorInitializerBlankLineRuleTests
{
    private readonly ConstructorInitializerBlankLineRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_colon_in_constructor_initializer"] = "false",
        });

    [Fact]
    public void Analyze_BlankLineAfterColon_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                C() :

                    base()
                {
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT231", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ColonAndBaseSameLine_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                C() : base()
                {
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ColonAndBaseNextLine_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                C() :
                    base()
                {
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThisInitializer_BlankLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                C(int x) :

                    this()
                {
                }

                C()
                {
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT231", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ConfigAllowed_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                C() :

                    base()
                {
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_allow_blank_line_after_colon_in_constructor_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                C() :

                    base()
                {
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
