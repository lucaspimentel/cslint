using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class UsingDeclarationRuleTests
{
    private readonly UsingDeclarationRule _rule = new();

    [Fact]
    public void Analyze_UsingBlockWithDeclaration_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    using (var x = new System.IO.MemoryStream())
                    {
                        x.WriteByte(0);
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_simple_using_statement"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT211", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SimpleUsingDeclaration_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    using var x = new System.IO.MemoryStream();
                    x.WriteByte(0);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_simple_using_statement"] = "true",
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
                    using (var x = new System.IO.MemoryStream())
                    {
                        x.WriteByte(0);
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_simple_using_statement"] = "false",
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
                    using (var x = new System.IO.MemoryStream())
                    {
                        x.WriteByte(0);
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NestedUsingStatements_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    using (var x = new System.IO.MemoryStream())
                    using (var y = new System.IO.MemoryStream())
                    {
                        x.WriteByte(0);
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_simple_using_statement"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
