using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SimplePropertyAccessorsRuleTests
{
    private readonly SimplePropertyAccessorsRule _rule = new();

    [Fact]
    public void Analyze_ExpressionBodiedGetSet_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int _name;
                public int Name { get => _name; set => _name = value; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0360", diagnostics[0].RuleId);
        Assert.Contains("Name", diagnostics[0].Message);
        Assert.Contains("_name", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_BlockBodiedGetSet_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int _name;
                public int Name
                {
                    get { return _name; }
                    set { _name = value; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_MixedBodies_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int _name;
                public int Name
                {
                    get => _name;
                    set { _name = value; }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_AutoProperty_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                public int Name { get; set; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_GetOnly_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private int _name;
                public int Name => _name;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_DifferentFields_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private int _a;
                private int _b;
                public int Name { get => _a; set => _b = value; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SetterWithExtraLogic_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                private int _name;
                public int Name
                {
                    get => _name;
                    set
                    {
                        _name = value;
                        Console.WriteLine("changed");
                    }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_InitAccessor_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int _name;
                public int Name { get => _name; init => _name = value; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("FALSE")]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics(string value)
    {
        string source = """
            class C
            {
                private int _name;
                public int Name { get => _name; set => _name = value; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_simple_property_accessors"] = value,
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
                private int _name;
                public int Name { get => _name; set => _name = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
