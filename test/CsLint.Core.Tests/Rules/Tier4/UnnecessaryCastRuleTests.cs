using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class UnnecessaryCastRuleTests
{
    private readonly UnnecessaryCastRule _rule = new();

    [Fact]
    public void Analyze_SameTypeCast_ReportsDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 0;
                    int y = (int)x;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0004", diagnostic.RuleId);
        Assert.Contains("int", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UpcastToBaseClass_ReportsDiagnostic()
    {
        const string source = """
            class Base { }
            class Derived : Base { }

            class C
            {
                void M()
                {
                    var d = new Derived();
                    Base b = (Base)d;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0004", diagnostic.RuleId);
        Assert.Contains("Base", diagnostic.Message);
    }

    [Fact]
    public void Analyze_UpcastToInterface_ReportsDiagnostic()
    {
        const string source = """
            using System;
            using System.IO;

            class C
            {
                void M()
                {
                    var stream = new MemoryStream();
                    IDisposable d = (IDisposable)stream;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("IDE0004", diagnostic.RuleId);
        Assert.Contains("IDisposable", diagnostic.Message);
    }

    [Fact]
    public void Analyze_Downcast_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    object obj = "hello";
                    string s = (string)obj;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NumericNarrowing_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    long l = 42;
                    int i = (int)l;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EnumToInt_NoDiagnostic()
    {
        const string source = """
            enum Color { Red, Green, Blue }

            class C
            {
                void M()
                {
                    Color c = Color.Red;
                    int i = (int)c;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NullableWrapping_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 5;
                    int? y = (int?)x;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_BoxingValueType_NoDiagnostic()
    {
        const string source = """
            class C
            {
                void M()
                {
                    int x = 5;
                    object o = (object)x;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UserDefinedConversion_NoDiagnostic()
    {
        const string source = """
            class Celsius
            {
                public double Degrees { get; }
                public Celsius(double d) => Degrees = d;
                public static implicit operator Fahrenheit(Celsius c) => new(c.Degrees * 9.0 / 5.0 + 32);
            }

            class Fahrenheit
            {
                public double Degrees { get; }
                public Fahrenheit(double d) => Degrees = d;
            }

            class C
            {
                void M()
                {
                    var c = new Celsius(100);
                    Fahrenheit f = (Fahrenheit)c;
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("(double)intVar / other", "int intVar = 10; int other = 3; double r = (double)intVar / other;")]
    [InlineData("(long)a + b", "int a = int.MaxValue; int b = 1; long r = (long)a + b;")]
    [InlineData("(float)x * y", "int x = 5; int y = 3; float r = (float)x * y;")]
    public void Analyze_WideningCastInArithmeticExpression_NoDiagnostic(string _, string statement)
    {
        string source = $$"""
            class C
            {
                void M()
                {
                    {{statement}}
                }
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("Write(short)", "writer.Write((short)1);")]
    [InlineData("Write(byte)", "writer.Write((byte)255);")]
    public void Analyze_CastForOverloadResolution_NoDiagnostic(string _, string statement)
    {
        string source = $$"""
            using System.IO;

            class C
            {
                void M()
                {
                    using var stream = new MemoryStream();
                    using var writer = new BinaryWriter(stream);
                    {{statement}}
                }
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
