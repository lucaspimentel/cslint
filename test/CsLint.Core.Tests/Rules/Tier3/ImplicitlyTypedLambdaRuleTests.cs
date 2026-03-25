using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ImplicitlyTypedLambdaRuleTests
{
    private readonly ImplicitlyTypedLambdaRule _rule = new();

    [Fact]
    public void Analyze_ExplicitTypes_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = (int x) => x + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_implicitly_typed_lambda_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0350", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultipleExplicitTypes_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, string, bool> f = (int x, string y) => true;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_implicitly_typed_lambda_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ImplicitTypes_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = (x) => x + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_implicitly_typed_lambda_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SimpleLambda_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = x => x + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_implicitly_typed_lambda_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExplicitReturnType_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    var f = int (int x) => x + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_implicitly_typed_lambda_expression"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NoParameters_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action a = () => Console.WriteLine();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_implicitly_typed_lambda_expression"] = "true",
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
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = (int x) => x + 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_implicitly_typed_lambda_expression"] = value,
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> f = (int x) => x + 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
