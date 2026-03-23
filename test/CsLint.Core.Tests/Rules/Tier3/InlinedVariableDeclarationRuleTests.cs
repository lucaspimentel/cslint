using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class InlinedVariableDeclarationRuleTests
{
    private readonly InlinedVariableDeclarationRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_inlined_variable_declaration"] = "true:warning",
        });

    [Fact]
    public void Analyze_SeparateDeclarationWithOutUsage_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x;
                    int.TryParse("1", out x);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT272", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_InlineOutVariable_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int.TryParse("1", out int x);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_DeclarationWithInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 0;
                    int.TryParse("1", out x);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_DeclarationNotUsedAsOut_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x;
                    x = 42;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x;
                    int.TryParse("1", out x);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
