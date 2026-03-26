using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ThreadStaticOnNonStaticFieldRuleTests
{
    private readonly ThreadStaticOnNonStaticFieldRule _rule = new();

    [Fact]
    public void Analyze_ThreadStaticOnInstanceField_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                [ThreadStatic]
                private int _field;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2259", diagnostic.RuleId);
        Assert.Contains("_field", diagnostic.Message);
    }

    [Fact]
    public void Analyze_ThreadStaticOnStaticField_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                [ThreadStatic]
                private static int _field;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NoThreadStaticAttribute_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int _field;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FullyQualifiedAttribute_ReturnsDiagnostic()
    {
        const string source = """
            class C
            {
                [System.ThreadStatic]
                private int _field;
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
            using System;

            class C
            {
                [ThreadStatic]
                private int _field;
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2259.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
