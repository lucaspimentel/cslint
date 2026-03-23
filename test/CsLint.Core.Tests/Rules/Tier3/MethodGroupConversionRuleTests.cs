using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class MethodGroupConversionRuleTests
{
    private readonly MethodGroupConversionRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_prefer_method_group_conversion"] = "true:warning",
        });

    [Fact]
    public void Analyze_SimpleLambdaForwarding_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using System.Linq;
            class C
            {
                int Parse(string s) => int.Parse(s);
                void M()
                {
                    var list = new[] { "1", "2" };
                    var result = list.Select(x => Parse(x));
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0200", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MultiParamLambdaForwarding_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                int Add(int a, int b) => a + b;
                void M()
                {
                    Func<int, int, int> f = (a, b) => Add(a, b);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_MethodGroup_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Linq;
            class C
            {
                void M()
                {
                    var list = new[] { "1", "2" };
                    var result = list.Select(int.Parse);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_LambdaWithExtraLogic_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Linq;
            class C
            {
                void M()
                {
                    var list = new[] { "1", "2" };
                    var result = list.Select(x => int.Parse(x) + 1);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_LambdaWithDifferentArgOrder_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                int Sub(int a, int b) => a - b;
                void M()
                {
                    Func<int, int, int> f = (a, b) => Sub(b, a);
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
                int Parse(string s) => int.Parse(s);
                void M()
                {
                    Func<string, int> f = x => Parse(x);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
