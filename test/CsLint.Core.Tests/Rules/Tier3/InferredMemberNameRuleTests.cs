using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class InferredMemberNameRuleTests
{
    private readonly InferredMemberNameRule _rule = new();

    [Fact]
    public void Analyze_SimpleIdentifierMatch_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int name = 1;
                    var t = (name: name, age: 2);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0037", diagnostics[0].RuleId);
        Assert.Contains("name", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_MemberAccessMatch_ReturnsDiagnostic()
    {
        string source = """
            class Person { public string Name { get; set; } public int Age { get; set; } }
            class C
            {
                void M()
                {
                    var person = new Person();
                    var t = (Name: person.Name, Age: person.Age);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.Contains("Name", diagnostics[0].Message);
        Assert.Contains("Age", diagnostics[1].Message);
    }

    [Fact]
    public void Analyze_TupleNameDoesNotMatchExpression_ReturnsNoDiagnostics()
    {
        string source = """
            class Person { public string Name { get; set; } }
            class C
            {
                void M()
                {
                    var person = new Person();
                    var t = (FirstName: person.Name, Count: 1);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NoExplicitTupleName_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int name = 1;
                    var t = (name, 2);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LiteralExpression_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    var t = (x: 1, y: 2);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TupleCaseMismatch_ReturnsNoDiagnostics()
    {
        string source = """
            class Person { public string Name { get; set; } }
            class C
            {
                void M()
                {
                    var person = new Person();
                    var t = (name: person.Name, age: 1);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NotInTupleExpression_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(int name)
                {
                    Other(name: name);
                }
                void Other(int name) { }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TupleConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int name = 1;
                    var t = (name: name, age: 2);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TupleConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int name = 1;
                    var t = (name: name, age: 2);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConditionalAccess_ReturnsDiagnostic()
    {
        string source = """
            class Person { public string Name { get; set; } }
            class C
            {
                void M()
                {
                    Person? person = null;
                    var t = (Name: person?.Name, Age: 1);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("Name", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_AnonymousType_RedundantName_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    var obj = new { x = x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0037", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_AnonymousType_DifferentName_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int y = 1;
                    var obj = new { x = y };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AnonymousType_MemberAccess_ReturnsNoDiagnostics()
    {
        string source = """
            class Obj { public int x; }
            class C
            {
                void M()
                {
                    var obj = new Obj();
                    var anon = new { x = obj.x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AnonymousType_CaseDiffers_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    var obj = new { X = x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AnonymousTypeConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    var obj = new { x = x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AnonymousTypeConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    var obj = new { x = x };
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
