using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class CollectionExpressionRuleTests
{
    private readonly CollectionExpressionRule _rule = new();

    [Fact]
    public void Analyze_ExplicitArrayCreation_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    var arr = new int[] { 1, 2, 3 };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_collection_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT222", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ImplicitArrayCreation_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    var arr = new[] { 1, 2, 3 };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_collection_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT222", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ArrayEmpty_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    var arr = Array.Empty<int>();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_collection_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT222", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_EnumerableEmpty_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    var arr = Enumerable.Empty<int>();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_collection_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT222", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ArrayWithoutInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    var arr = new int[5];
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_collection_expression"] = "true",
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
                    var arr = new int[] { 1, 2, 3 };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_collection_expression"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
