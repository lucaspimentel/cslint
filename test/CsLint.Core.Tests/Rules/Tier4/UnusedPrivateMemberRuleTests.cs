using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnusedPrivateMemberRuleTests
{
    private readonly UnusedPrivateMemberRule _rule = new();

    [Fact]
    public void Analyze_UnusedPrivateField_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                private int _unused;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT308", diagnostic.RuleId);
        Assert.Contains("_unused", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UnusedPrivateMethod_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                private void Unused() { }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT308", diagnostic.RuleId);
        Assert.Contains("Unused", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UnusedPrivateProperty_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                private int Unused { get; set; }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT308", diagnostic.RuleId);
        Assert.Contains("Unused", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UnusedPrivateEvent_ReportsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                private event EventHandler Unused;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT308", diagnostic.RuleId);
        Assert.Contains("Unused", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UsedPrivateField_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int _field;

                public int GetField() => _field;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UsedPrivateMethod_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int Helper() => 42;

                public int GetValue() => Helper();
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UsedPrivateProperty_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int Prop { get; set; }

                public int GetProp() => Prop;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PublicAndInternalMembers_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public int PublicField;
                internal int InternalField;
                protected int ProtectedField;

                public void PublicMethod() { }
                internal void InternalMethod() { }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PartialClassMembers_NoDiagnostic()
    {
        const string source = """
            partial class C
            {
                private int _field;
                private void Method() { }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_DiscardField_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int _;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MissingReferences_NoDiagnostics()
    {
        const string source = """
            using Some.Missing.Namespace;

            class C
            {
                private int _unused;
            }
            """;

        RuleContext context = TestHelper.CreateContext(source);
        // Create compilation without references to trigger CS0246
        SyntaxTree tree = context.SyntaxTree;
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Test",
            [tree],
            [],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        SemanticModel model = compilation.GetSemanticModel(tree);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PrivateFieldUsedViaThis_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int _field;

                public int GetField() => this._field;
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
