using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class StaticHolderShouldBeSealedRuleTests
{
    private readonly StaticHolderShouldBeSealedRule _rule = new();

    [Fact]
    public void Analyze_AllStaticMembers_NotSealed_ReturnsDiagnostic()
    {
        const string source = """
            class Utility
            {
                public static void DoWork() { }
                public static int Value => 42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1052", diagnostic.RuleId);
        Assert.Contains("Utility", diagnostic.Message);
    }

    [Fact]
    public void Analyze_ConstFieldsAndStaticMethods_ReturnsDiagnostic()
    {
        const string source = """
            class Constants
            {
                public const int Max = 100;
                public static void Log() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_AlreadySealed_NoDiagnostic()
    {
        const string source = """
            sealed class Utility
            {
                public static void DoWork() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AlreadyStatic_NoDiagnostic()
    {
        const string source = """
            static class Utility
            {
                public static void DoWork() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_HasInstanceMember_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public static void DoWork() { }
                public int Value { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_HasInstanceConstructor_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public C() { }
                public static void DoWork() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_HasBaseType_NoDiagnostic()
    {
        const string source = """
            class Base { }

            class Derived : Base
            {
                public static void DoWork() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        // Only Derived has a base type; Base is empty so not a static holder
        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EmptyClass_NoDiagnostic()
    {
        const string source = "class C { }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AbstractClass_NoDiagnostic()
    {
        const string source = """
            abstract class C
            {
                public static void DoWork() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_StaticCtorOnly_ReturnsDiagnostic()
    {
        const string source = """
            class C
            {
                static C() { }
                public static void DoWork() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            class Utility
            {
                public static void DoWork() { }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1052.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
