using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnreadPrivateMemberRuleTests
{
    private readonly UnreadPrivateMemberRule _rule = new();

    [Fact]
    public void Analyze_FieldWrittenButNeverRead_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                private int _field;

                public void Set(int value) { _field = value; }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0052", diagnostic.RuleId);
        Assert.Contains("_field", diagnostic.Message);
    }

    [Fact]
    public void Analyze_FieldWrittenAndRead_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int _field;

                public void Set(int value) { _field = value; }
                public int Get() => _field;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FieldNeverReferenced_NoDiagnostic()
    {
        // Completely unused fields are IDE0051's responsibility, not IDE0052
        const string source = """
            class C
            {
                private int _unused;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FieldOnlyReadNotWritten_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int _field = 42;

                public int Get() => _field;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FieldWithCompoundAssignment_NoDiagnostic()
    {
        // Compound assignment (+=) is both read and write
        const string source = """
            class C
            {
                private int _counter;

                public void Increment() { _counter += 1; }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FieldPassedAsOut_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                private int _value;

                public void Parse(string s) { int.TryParse(s, out _value); }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0052", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_PublicField_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public int Field;

                public void Set(int value) { Field = value; }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PartialClass_NoDiagnostic()
    {
        const string source = """
            partial class C
            {
                private int _field;

                public void Set(int value) { _field = value; }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    private static (RuleContext Context, SemanticModel Model) CreateSemanticContext(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);
        SemanticModel model = CompilationFactory.CreateSemanticModel(context.SyntaxTree);
        return (context, model);
    }
}
