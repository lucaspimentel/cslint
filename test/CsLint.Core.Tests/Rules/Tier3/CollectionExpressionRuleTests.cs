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
    public void Analyze_ImplicitArrayWithMethodChain_ReturnsDiagnosticWithChainMessage()
    {
        string source = """
            using System.Collections.Frozen;
            class C
            {
                FrozenSet<string> Keywords { get; } = new[] { "abstract", "sealed" }.ToFrozenSet();
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
        Assert.Contains("target-typed collection expression", diagnostics[0].Message);
        Assert.Contains("method chain", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ExplicitArrayWithMethodChain_ReturnsDiagnosticWithChainMessage()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                List<string> Items { get; } = new string[] { "a", "b" }.ToList();
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
        Assert.Contains("target-typed collection expression", diagnostics[0].Message);
        Assert.Contains("method chain", diagnostics[0].Message);
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

    [Theory]
    [InlineData("new[] { \"/some/path\" }")]
    [InlineData("new string[] { \"a\", \"b\" }")]
    public void Analyze_ArrayInAttribute_ReturnsNoDiagnostics(string arrayExpr)
    {
        string source = $$"""
            using System;
            class C
            {
                [InlineData({{arrayExpr}})]
                void M() { }
            }
            class InlineDataAttribute(params object[] args) : Attribute { }
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
