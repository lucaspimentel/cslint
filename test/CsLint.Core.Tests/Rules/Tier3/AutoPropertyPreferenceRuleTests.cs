using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class AutoPropertyPreferenceRuleTests
{
    private readonly AutoPropertyPreferenceRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string> { ["dotnet_style_prefer_auto_properties"] = "true" });

    [Fact]
    public void SimpleGetSet_Flagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X { get => _x; set => _x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0032", diagnostics[0].RuleId);
        Assert.Contains("X", diagnostics[0].Message);
    }

    [Fact]
    public void ExpressionBodiedGetterOnly_Flagged()
    {
        string source = """
            class C
            {
                private readonly int _x;
                public int X => _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void BlockBodiedGetterOnly_Flagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X { get { return _x; } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void BlockBodiedGetSet_Flagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X
                {
                    get { return _x; }
                    set { _x = value; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void ThisQualified_Flagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X { get => this._x; set => this._x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void FieldUsedInMethod_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X { get => _x; set => _x = value; }
                public void Reset() { _x = 0; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void FieldUsedInConstructor_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X { get => _x; set => _x = value; }
                public C() { _x = 42; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void FieldWithAttributes_NotFlagged()
    {
        string source = """
            using System;
            class C
            {
                [NonSerialized]
                private int _x;
                public int X { get => _x; set => _x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PartialType_NotFlagged()
    {
        string source = """
            partial class C
            {
                private int _x;
                public int X { get => _x; set => _x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PublicField_NotFlagged()
    {
        string source = """
            class C
            {
                public int _x;
                public int X { get => _x; set => _x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void GetterDoesComputation_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X { get => _x * 2; set => _x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void SetterDoesValidation_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X
                {
                    get => _x;
                    set
                    {
                        if (value < 0) throw new System.ArgumentException();
                        _x = value;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void GetterAndSetterDifferentFields_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                private int _y;
                public int X { get => _x; set => _y = value; }
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
                private int _x;
                public int X { get => _x; set => _x = value; }
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
                private int _x;
                public int X { get => _x; set => _x = value; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_auto_properties"] = "false",
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
            ["dotnet_style_prefer_auto_properties"] = value,
        });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_WhenConfigAbsent_ReturnsFalse()
    {
        Assert.False(_rule.IsEnabled(LintConfiguration.Empty));
    }

    [Fact]
    public void StaticField_NotFlagged()
    {
        string source = """
            class C
            {
                private static int _x;
                public int X { get => _x; set => _x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void InitAccessor_Flagged()
    {
        string source = """
            class C
            {
                private int _x;
                public int X { get => _x; init => _x = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void MultipleProperties_OnlySimpleOnesFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                private int _y;
                public int X { get => _x; set => _x = value; }
                public int Y { get => _y * 2; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("X", diagnostics[0].Message);
    }

    [Fact]
    public void AutoPropertyAlready_NotFlagged()
    {
        string source = """
            class C
            {
                public int X { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
