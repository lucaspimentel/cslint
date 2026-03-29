using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class PreferForeachExplicitCastRuleTests
{
    private readonly PreferForeachExplicitCastRule _rule = new();

    [Fact]
    public void Analyze_NonGenericCollectionWithSpecificType_Always_ReportsDiagnostic()
    {
        const string source = """
            using System.Collections;

            class C
            {
                void M(IEnumerable collection)
                {
                    foreach (string item in collection) { }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source, preference: "always");
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0220", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_NonGenericCollection_WhenStronglyTyped_NoDiagnostic()
    {
        const string source = """
            using System.Collections;

            class C
            {
                void M(IEnumerable collection)
                {
                    foreach (string item in collection) { }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source, preference: "when_strongly_typed");
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_GenericCollectionWithDifferentType_ReportsDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;

            class C
            {
                void M(List<object> list)
                {
                    foreach (string item in list) { }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0220", diagnostic.RuleId);
        Assert.Contains("object", diagnostic.Message);
        Assert.Contains("string", diagnostic.Message);
    }

    [Fact]
    public void Analyze_MatchingGenericType_NoDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;

            class C
            {
                void M(List<string> list)
                {
                    foreach (string item in list) { }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_VarIterationVariable_NoDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;

            class C
            {
                void M(List<string> list)
                {
                    foreach (var item in list) { }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_DerivedToBase_NoDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;

            class Base { }
            class Derived : Base { }

            class C
            {
                void M(List<Derived> list)
                {
                    foreach (Base item in list) { }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IEnumerableOfT_WithDifferentType_ReportsDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;

            class C
            {
                void M(IEnumerable<object> items)
                {
                    foreach (string item in items) { }
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0220", diagnostic.RuleId);
    }

    private static (RuleContext Context, SemanticModel Model) CreateSemanticContext(
        string source,
        string preference = "when_strongly_typed")
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.IDE0220.severity"] = "warning",
            ["dotnet_style_prefer_foreach_explicit_cast_in_source"] = preference,
        });

        RuleContext context = TestHelper.CreateContext(source, configuration: config);
        SemanticModel model = CompilationFactory.CreateSemanticModel(context.SyntaxTree);
        return (context, model);
    }
}
