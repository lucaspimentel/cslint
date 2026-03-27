using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ReadonlyStructPreferenceRuleTests
{
    private readonly ReadonlyStructPreferenceRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string> { ["csharp_style_prefer_readonly_struct"] = "true" });

    [Fact]
    public void AllReadonlyFields_NoMethods_Flagged()
    {
        string source = """
            struct S
            {
                public readonly int X;
                public readonly string Y;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0250", diagnostics[0].RuleId);
        Assert.Contains("S", diagnostics[0].Message);
    }

    [Fact]
    public void AllReadonlyFields_NonMutatingMethod_Flagged()
    {
        string source = """
            struct S
            {
                public readonly int X;
                public int GetX() => X;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void NoInstanceFields_NonMutatingMethod_Flagged()
    {
        string source = """
            struct S
            {
                public static int Count;
                public int GetDefault() => 0;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void NonReadonlyField_NotFlagged()
    {
        string source = """
            struct S
            {
                public int X;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MethodMutatesField_NotFlagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public void Set(int v) { _x = v; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MethodAssignsThis_NotFlagged()
    {
        string source = """
            struct S
            {
                public readonly int X;
                public void Reset() { this = default; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void AlreadyReadonlyStruct_NotFlagged()
    {
        string source = """
            readonly struct S
            {
                public readonly int X;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void RefStruct_NotFlagged()
    {
        string source = """
            ref struct S
            {
                public readonly int X;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PartialStruct_NotFlagged()
    {
        string source = """
            partial struct S
            {
                public readonly int X;
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
            struct S
            {
                public readonly int X;
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
            struct S
            {
                public readonly int X;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_readonly_struct"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ConstructorAssignment_StillFlagged()
    {
        // Constructor assignments are fine — struct can still be readonly
        string source = """
            struct S
            {
                public readonly int X;
                public S(int x) { X = x; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Constructors are allowed to assign fields in readonly structs
        Assert.Single(diagnostics);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void IsEnabled_WhenConfigPresent_ReturnsTrue(string value)
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_readonly_struct"] = value,
        });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_WhenConfigAbsent_ReturnsFalse()
    {
        Assert.False(_rule.IsEnabled(LintConfiguration.Empty));
    }

    [Fact]
    public void MixedReadonlyAndNonReadonlyFields_NotFlagged()
    {
        string source = """
            struct S
            {
                public readonly int X;
                public int Y;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PropertyIncrementingField_NotFlagged()
    {
        string source = """
            struct S
            {
                private int _count;
                public int Count { get => _count; set => _count = value; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
