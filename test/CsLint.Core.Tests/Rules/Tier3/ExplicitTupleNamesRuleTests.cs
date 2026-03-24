using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExplicitTupleNamesRuleTests
{
    private readonly ExplicitTupleNamesRule _rule = new();

    [Fact]
    public void Analyze_LocalVar_NamedTuple_ItemAccess_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = ("Alice", 30);
                    var n = person.Item1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0033", diagnostics[0].RuleId);
        Assert.Contains("name", diagnostics[0].Message);
        Assert.Contains("Item1", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_LocalVar_NamedTuple_MultipleItemAccess_ReturnsMultipleDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = ("Alice", 30);
                    var n = person.Item1;
                    var a = person.Item2;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.Contains("name", diagnostics[0].Message);
        Assert.Contains("age", diagnostics[1].Message);
    }

    [Fact]
    public void Analyze_LocalVar_NamedTuple_NameAccess_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = ("Alice", 30);
                    var n = person.name;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LocalVar_UnnamedTuple_ItemAccess_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string, int) pair = ("Alice", 30);
                    var n = pair.Item1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Parameter_NamedTuple_ItemAccess_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M((string name, int age) person)
                {
                    var n = person.Item1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("name", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ForEach_NamedTuple_ItemAccess_ReturnsDiagnostic()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                void M(List<(string name, int age)> people)
                {
                    foreach ((string name, int age) person in people)
                    {
                        var n = person.Item1;
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("name", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ItemIndexOutOfRange_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = ("Alice", 30);
                    var x = person.Item3;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
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
                    (string name, int age) person = ("Alice", 30);
                    var n = person.Item1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = ("Alice", 30);
                    var n = person.Item1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MemberAccess_NotIdentifier_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                (string name, int age) GetPerson() => ("Alice", 30);

                void M()
                {
                    var n = GetPerson().Item1;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NonItemMemberAccess_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = ("Alice", 30);
                    var s = person.ToString();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_explicit_tuple_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
