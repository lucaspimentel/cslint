using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class CompoundAssignmentRuleTests
{
    private readonly CompoundAssignmentRule _rule = new();

    [Theory]
    [InlineData("x = x + 1", "+=")]
    [InlineData("x = x - 1", "-=")]
    [InlineData("x = x * 2", "*=")]
    [InlineData("x = x / 2", "/=")]
    [InlineData("x = x % 3", "%=")]
    [InlineData("x = x & 0xFF", "&=")]
    [InlineData("x = x | 0xFF", "|=")]
    [InlineData("x = x ^ 0xFF", "^=")]
    [InlineData("x = x << 2", "<<=")]
    [InlineData("x = x >> 2", ">>=")]
    public void Analyze_SimpleAssignmentWithBinaryOp_ReturnsDiagnostic(string assignment, string expectedOp)
    {
        string source = $$"""
            class C
            {
                void M()
                {
                    int x = 10;
                    {{assignment}};
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_compound_assignment"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT214", diagnostics[0].RuleId);
        Assert.Contains(expectedOp, diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_CoalesceAssignment_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    string x = null;
                    x = x ?? "default";
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_compound_assignment"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("??=", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_DifferentVariables_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 10;
                    int y = 5;
                    x = y + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_compound_assignment"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AlreadyCompound_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 10;
                    x += 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_compound_assignment"] = "true",
        });
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
                void M()
                {
                    int x = 10;
                    x = x + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_compound_assignment"] = "false",
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
                void M()
                {
                    int x = 10;
                    x = x + 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
