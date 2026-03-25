using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class StaticLocalFunctionRuleTests
{
    private readonly StaticLocalFunctionRule _rule = new();

    [Fact]
    public void Analyze_NonStaticWithNoCaptures_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    void Local() { int x = 1; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0062", diagnostics[0].RuleId);
        Assert.Contains("Local", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_AlreadyStatic_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    static void Local() { int x = 1; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_CapturesLocalVariable_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    void Local() { int y = x; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_CapturesParameter_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    void Local() { int y = x; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UsesThis_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                int _field;
                void M()
                {
                    void Local() { int y = this._field; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UsesBase_ReturnsNoDiagnostics()
    {
        string source = """
            class B { public virtual void F() { } }
            class C : B
            {
                void M()
                {
                    void Local() { base.F(); }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
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
            class C
            {
                void M()
                {
                    void Local() { int x = 1; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = value,
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
                    void Local() { int x = 1; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LocalShadowsEnclosingVariable_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    void Local() { int x = 2; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // The local declares its own 'x', so it doesn't capture the enclosing one
        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ExpressionBodiedLocalFunction_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Local() => 42;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ExpressionBodiedCapturesParameter_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    int Local() => x;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UsesOwnParameter_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int Local(int x) { return x + 1; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_NestedLocalFunction_CapturesOuterLocal_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    void Outer()
                    {
                        void Inner() { int y = x; }
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Outer captures x, Inner also captures x from M's scope
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleLocalFunctions_MixedCapture_ReportsCorrectly()
    {
        string source = """
            class C
            {
                void M(int x)
                {
                    void NoCap() { int y = 1; }
                    void Cap() { int y = x; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_static_local_function"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("NoCap", diagnostics[0].Message);
    }
}
