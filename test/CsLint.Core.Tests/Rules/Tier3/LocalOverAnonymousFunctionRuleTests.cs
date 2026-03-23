using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class LocalOverAnonymousFunctionRuleTests
{
    private readonly LocalOverAnonymousFunctionRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_prefer_local_over_anonymous_function"] = "true:warning",
        });

    [Fact]
    public void Analyze_LambdaAssignedToDelegate_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> square = x => x * x;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT276", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ActionLambda_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action doWork = () => Console.WriteLine("hi");
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AnonymousMethod_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Func<int> f = delegate { return 42; };
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_LocalFunction_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Square(int x) => x * x;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_VarLambda_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    var square = (int x) => x * x;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NonLambdaInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 42;
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
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> square = x => x * x;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
