using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PrimaryConstructorRuleTests
{
    private readonly PrimaryConstructorRule _rule = new();

    [Fact]
    public void Analyze_SimpleFieldAssignment_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private readonly int _x;

                public C(int x)
                {
                    _x = x;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_primary_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT221", diagnostics[0].RuleId);
        Assert.Contains("primary constructor", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_MultipleConstructors_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private readonly int _x;

                public C(int x)
                {
                    _x = x;
                }

                public C() : this(0)
                {
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_primary_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConstructorWithLogic_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private readonly int _x;

                public C(int x)
                {
                    _x = x;
                    System.Console.WriteLine(x);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_primary_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConstructorWithBaseInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            class C : Base
            {
                private readonly int _x;

                public C(int x) : base(x)
                {
                    _x = x;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_primary_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ParameterlessConstructor_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                public C()
                {
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_primary_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThisQualifiedAssignment_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int _x;

                public C(int x)
                {
                    this._x = x;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_primary_constructors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private readonly int _x;

                public C(int x)
                {
                    _x = x;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_primary_constructors"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
