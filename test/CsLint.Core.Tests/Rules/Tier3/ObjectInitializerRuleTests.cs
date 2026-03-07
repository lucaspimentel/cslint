using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ObjectInitializerRuleTests
{
    private readonly ObjectInitializerRule _rule = new();

    [Fact]
    public void Analyze_NewFollowedByPropertyAssignments_ReturnsDiagnostic()
    {
        string source = """
            class Foo { public int A { get; set; } public int B { get; set; } }
            class C
            {
                void M()
                {
                    var x = new Foo();
                    x.A = 1;
                    x.B = 2;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_object_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT215", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NewWithExistingInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo { public int A { get; set; } }
            class C
            {
                void M()
                {
                    var x = new Foo { A = 1 };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_object_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NewWithNoFollowingAssignments_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo { public int A { get; set; } }
            class C
            {
                void M()
                {
                    var x = new Foo();
                    var y = 42;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_object_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AssignmentToDifferentVariable_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo { public int A { get; set; } }
            class C
            {
                void M()
                {
                    var x = new Foo();
                    Console.WriteLine(x);
                    y.A = 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_object_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo { public int A { get; set; } }
            class C
            {
                void M()
                {
                    var x = new Foo();
                    x.A = 1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_object_initializer"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo { public int A { get; set; } }
            class C
            {
                void M()
                {
                    var x = new Foo();
                    x.A = 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
