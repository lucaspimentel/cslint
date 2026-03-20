using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ElementOrderRuleTests
{
    private readonly ElementOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_element_ordering"] = "true",
        });

    [Fact]
    public void Analyze_CorrectOrder_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private const int C = 1;
                private int _x;
                public Foo() { }
                public int X { get; set; }
                public void DoSomething() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MethodBeforeProperty_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                public void DoSomething() { }
                public int X { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT268", diagnostics[0].RuleId);
        Assert.Contains("property", diagnostics[0].Message);
        Assert.Contains("method", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_NestedTypesAtEnd_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private int _x;
                public void DoSomething() { }
                private class Inner { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConstructorBeforeField_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                public Foo() { }
                private int _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT268", diagnostics[0].RuleId);
        Assert.Contains("field", diagnostics[0].Message);
        Assert.Contains("constructor", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ConstBeforeField_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private const int C = 1;
                private int _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EventBeforeProperty_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class Foo
            {
                public event EventHandler E;
                public int X { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            class Foo
            {
                public void DoSomething() { }
                public int X { get; set; }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_element_ordering"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
