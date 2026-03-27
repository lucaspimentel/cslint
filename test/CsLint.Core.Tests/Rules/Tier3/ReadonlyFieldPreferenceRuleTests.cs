using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ReadonlyFieldPreferenceRuleTests
{
    private readonly ReadonlyFieldPreferenceRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string> { ["dotnet_style_readonly_field"] = "true" });

    [Fact]
    public void FieldAssignedOnlyInConstructor_Flagged()
    {
        string source = """
            class C
            {
                private int _x;
                public C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0044", diagnostics[0].RuleId);
        Assert.Contains("_x", diagnostics[0].Message);
    }

    [Fact]
    public void FieldAssignedOnlyInInitializer_Flagged()
    {
        string source = """
            class C
            {
                private int _x = 42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void FieldAssignedInInitializerAndConstructor_Flagged()
    {
        string source = """
            class C
            {
                private int _x = 0;
                public C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void FieldAssignedInMethod_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public C() { _x = 1; }
                public void Set(int v) { _x = v; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void FieldAssignedInPropertySetter_NotFlagged()
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

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void FieldPassedByRef_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public C() { _x = 1; }
                public void M() { Modify(ref _x); }
                static void Modify(ref int v) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void FieldPassedByOut_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public void M() { TryGet(out _x); }
                static void TryGet(out int v) { v = 0; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void AlreadyReadonly_NotFlagged()
    {
        string source = """
            class C
            {
                private readonly int _x = 1;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ConstField_NotFlagged()
    {
        string source = """
            class C
            {
                private const int X = 1;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void VolatileField_NotFlagged()
    {
        string source = """
            class C
            {
                private volatile int _x;
                public C() { _x = 1; }
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
                public int X;
                public C() { X = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void InternalField_NotFlagged()
    {
        string source = """
            class C
            {
                internal int _x;
                public C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ProtectedField_NotFlagged()
    {
        string source = """
            class C
            {
                protected int _x;
                public C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PartialClass_NotFlagged()
    {
        string source = """
            partial class C
            {
                private int _x;
                public C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void StaticFieldInStaticConstructor_Flagged()
    {
        string source = """
            class C
            {
                private static int _x;
                static C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void StaticFieldInInstanceConstructor_NotFlagged()
    {
        string source = """
            class C
            {
                private static int _x;
                public C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void InstanceFieldInStaticConstructor_NotFlagged()
    {
        // This would actually be a compile error, but we should handle it gracefully
        string source = """
            class C
            {
                private int _x;
                static C() { }
                void M() { _x = 1; }
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
                public C() { _x = 1; }
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
                public C() { _x = 1; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_readonly_field"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ThisQualifiedAssignment_InConstructor_Flagged()
    {
        string source = """
            class C
            {
                private int _x;
                public C() { this._x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void ThisQualifiedAssignment_InMethod_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                void M() { this._x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void CompoundAssignment_InMethod_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public C() { _x = 1; }
                void M() { _x += 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PostIncrement_InMethod_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public C() { _x = 1; }
                void M() { _x++; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PreDecrement_InMethod_NotFlagged()
    {
        string source = """
            class C
            {
                private int _x;
                public C() { _x = 1; }
                void M() { --_x; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void NestedTypeAssignsParentField_NotFlagged()
    {
        string source = """
            class Outer
            {
                private int _x;
                public Outer() { _x = 1; }
                class Inner
                {
                    void M(Outer o) { o._x = 2; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // The assignment in Inner is `o._x`, not `_x` or `this._x`,
        // so it won't match our field name pattern — but the nested
        // type's constructor context check should handle direct access
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void FieldAssignedInLambdaInsideConstructor_NotFlagged()
    {
        string source = """
            using System;
            class C
            {
                private int _x;
                public C()
                {
                    Action a = () => _x = 1;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void DefaultAccessModifier_IsTreatedAsPrivate()
    {
        // No access modifier = private by default in C#
        string source = """
            class C
            {
                int _x;
                public C() { _x = 1; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void MultipleFieldsOnOneLine_EachAnalyzedIndependently()
    {
        string source = """
            class C
            {
                private int _x, _y;
                public C() { _x = 1; }
                void M() { _y = 2; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("_x", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void IsEnabled_WhenConfigPresent_ReturnsTrue(string value)
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_readonly_field"] = value,
        });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_WhenConfigAbsent_ReturnsFalse()
    {
        Assert.False(_rule.IsEnabled(LintConfiguration.Empty));
    }

    [Fact]
    public void StructField_AssignedOnlyInConstructor_Flagged()
    {
        string source = """
            struct S
            {
                private int _x;
                public S(int x) { _x = x; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void NoFieldsInType_NoDiagnostics()
    {
        string source = """
            class C
            {
                public int Prop { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
