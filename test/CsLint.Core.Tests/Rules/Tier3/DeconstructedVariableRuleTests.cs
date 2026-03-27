using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class DeconstructedVariableRuleTests
{
    private readonly DeconstructedVariableRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string> { ["csharp_style_deconstructed_variable_declaration"] = "true" });

    [Fact]
    public void ExplicitTupleType_ElementAccessed_Flagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = GetPerson();
                    var n = person.name;
                }
                static (string, int) GetPerson() => ("Alice", 30);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0042", diagnostics[0].RuleId);
        Assert.Contains("person", diagnostics[0].Message);
    }

    [Fact]
    public void ExplicitTupleType_MultipleElementsAccessed_Flagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = GetPerson();
                    var n = person.name;
                    var a = person.age;
                }
                static (string, int) GetPerson() => ("Alice", 30);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void ExplicitTupleType_NoElementAccessed_NotFlagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = GetPerson();
                    DoSomething(person);
                }
                static (string, int) GetPerson() => ("Alice", 30);
                static void DoSomething(object o) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void AlreadyDeconstructed_NotFlagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    var (name, age) = GetPerson();
                    var n = name;
                }
                static (string, int) GetPerson() => ("Alice", 30);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void UnnamedTupleElements_NotFlagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string, int) t = GetPerson();
                    var n = t.Item1;
                }
                static (string, int) GetPerson() => ("Alice", 30);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void VariableReassigned_NotFlagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = GetPerson();
                    person = GetPerson();
                    var n = person.name;
                }
                static (string, int) GetPerson() => ("Alice", 30);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ConfigAbsent_NotFlagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = GetPerson();
                    var n = person.name;
                }
                static (string, int) GetPerson() => ("Alice", 30);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ConfigFalse_NotFlagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string name, int age) person = GetPerson();
                    var n = person.name;
                }
                static (string, int) GetPerson() => ("Alice", 30);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_deconstructed_variable_declaration"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void IsEnabled_WhenConfigPresent_ReturnsTrue(string value)
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_deconstructed_variable_declaration"] = value,
        });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_WhenConfigAbsent_ReturnsFalse()
    {
        Assert.False(_rule.IsEnabled(LintConfiguration.Empty));
    }

    [Fact]
    public void ThreeElementTuple_OneAccessed_Flagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    (string first, string last, int age) person = GetPerson();
                    var f = person.first;
                }
                static (string, string, int) GetPerson() => ("A", "B", 30);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void NonTupleType_NotFlagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    string name = "Alice";
                    var n = name.Length;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
