using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PropertySelfAssignmentInSetterRuleTests
{
    private readonly PropertySelfAssignmentInSetterRule _rule = new();

    [Fact]
    public void Analyze_SelfAssignmentInSetter_ReturnsDiagnostic()
    {
        const string source = """
            class C
            {
                private int _value;

                public int Value
                {
                    get => _value;
                    set { Value = value; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2011", diagnostic.RuleId);
        Assert.Contains("Value", diagnostic.Message);
    }

    [Fact]
    public void Analyze_SelfAssignmentInExpressionBodiedSetter_ReturnsDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value
                {
                    get => 0;
                    set => Value = value;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2011", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_BackingFieldAssignment_NoDiagnostic()
    {
        const string source = """
            class C
            {
                private int _value;

                public int Value
                {
                    get => _value;
                    set { _value = value; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AutoProperty_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SelfAssignmentInInitAccessor_ReturnsDiagnostic()
    {
        const string source = """
            class C
            {
                public int Value
                {
                    get => 0;
                    init { Value = value; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2011", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_ExplicitInterfaceImplementation_NoDiagnostic()
    {
        const string source = """
            interface IFoo { int Value { get; set; } }

            class C : IFoo
            {
                public int Value { get; set; }

                int IFoo.Value
                {
                    get => Value;
                    set => Value = value;
                }
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
            class C
            {
                public int Value
                {
                    get => 0;
                    set { Value = value; }
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2011.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
