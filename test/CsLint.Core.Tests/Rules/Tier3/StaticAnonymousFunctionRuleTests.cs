using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class StaticAnonymousFunctionRuleTests
{
    private readonly StaticAnonymousFunctionRule _rule = new();

    [Fact]
    public void Analyze_SimpleLambdaNoCaptures_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action<int> a = x => Console.WriteLine(x);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0320", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ParenthesizedLambdaNoCaptures_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action<int> a = (x) => Console.WriteLine(x);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_AnonymousMethodNoCaptures_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action<int> a = delegate(int x) { Console.WriteLine(x); };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_AlreadyStatic_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action<int> a = static x => Console.WriteLine(x);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_CapturesLocalVariable_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    int y = 1;
                    Action a = () => Console.WriteLine(y);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_CapturesParameter_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                void M(int y)
                {
                    Action a = () => Console.WriteLine(y);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UsesThis_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                int _field;
                void M()
                {
                    Action a = () => Console.WriteLine(this._field);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
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
                    Action<int> a = x => Console.WriteLine(x);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = value,
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
                    Action<int> a = x => Console.WriteLine(x);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ParameterShadowsEnclosing_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    int x = 1;
                    Action<int> a = (x) => Console.WriteLine(x);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Parameter 'x' shadows enclosing 'x', so no actual capture
        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ParameterlessLambdaNoCaptures_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class C
            {
                void M()
                {
                    Action a = () => Console.WriteLine(42);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleLambdas_MixedCapture_ReportsCorrectly()
    {
        string source = """
            using System;
            class C
            {
                void M(int y)
                {
                    Action<int> noCap = x => Console.WriteLine(x);
                    Action cap = () => Console.WriteLine(y);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_anonymous_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }
}
