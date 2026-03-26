using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ThreadStaticWithInitializerRuleTests
{
    private readonly ThreadStaticWithInitializerRule _rule = new();

    [Fact]
    public void Analyze_ThreadStaticWithInitializer_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                [ThreadStatic]
                private static int _field = 42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2019", diagnostic.RuleId);
        Assert.Contains("_field", diagnostic.Message);
    }

    [Fact]
    public void Analyze_ThreadStaticWithoutInitializer_NoDiagnostic()
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
    public void Analyze_ThreadStaticOnNonStaticField_NoDiagnostic()
    {
        // CA2259 handles non-static; this rule only fires for static fields
        const string source = """
            using System;

            class C
            {
                [ThreadStatic]
                private int _field = 42;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                [ThreadStatic]
                private static int _field = 42;
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2019.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
