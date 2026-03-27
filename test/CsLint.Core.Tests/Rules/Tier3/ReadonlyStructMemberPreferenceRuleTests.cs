using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ReadonlyStructMemberPreferenceRuleTests
{
    private readonly ReadonlyStructMemberPreferenceRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string> { ["csharp_style_prefer_readonly_struct_member"] = "true" });

    [Fact]
    public void NonMutatingMethod_Flagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public int GetX() => _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0251", diagnostics[0].RuleId);
        Assert.Contains("GetX", diagnostics[0].Message);
    }

    [Fact]
    public void NonMutatingPropertyGetter_Flagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public int X { get => _x; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("X", diagnostics[0].Message);
    }

    [Fact]
    public void ExpressionBodiedProperty_NonMutating_Flagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public int X => _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void MethodAssignsField_NotFlagged()
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
    public void MethodIncrementsField_NotFlagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public void Increment() { _x++; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PropertyWithSetter_NotFlagged()
    {
        string source = """
            struct S
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
    public void AlreadyReadonlyMethod_NotFlagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public readonly int GetX() => _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void StaticMethod_NotFlagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public static int GetDefault() => 0;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ReadonlyStruct_AllMembersImplicitlyReadonly_NotFlagged()
    {
        string source = """
            readonly struct S
            {
                private readonly int _x;
                public int GetX() => _x;
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
                private int _x;
                public int GetX() => _x;
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
                private int _x;
                public int GetX() => _x;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_readonly_struct_member"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MultipleNonMutatingMembers_MultipleDiagnostics()
    {
        string source = """
            struct S
            {
                private int _x;
                private int _y;
                public int GetX() => _x;
                public int GetY() => _y;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void MixedMembers_OnlyNonMutatingFlagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public int GetX() => _x;
                public void SetX(int v) { _x = v; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("GetX", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void IsEnabled_WhenConfigPresent_ReturnsTrue(string value)
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_readonly_struct_member"] = value,
        });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_WhenConfigAbsent_ReturnsFalse()
    {
        Assert.False(_rule.IsEnabled(LintConfiguration.Empty));
    }

    [Fact]
    public void MethodOnClass_NotAnalyzed()
    {
        // IDE0251 only applies to struct members
        string source = """
            class C
            {
                private int _x;
                public int GetX() => _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MethodWithRefFieldArgument_NotFlagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public void M() { Modify(ref _x); }
                static void Modify(ref int v) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
