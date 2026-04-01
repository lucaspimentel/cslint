using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public sealed class UnnecessaryInitializationRuleTests
{
    private readonly UnnecessaryInitializationRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1805.severity"] = "warning",
    });

    [Theory]
    [InlineData("int", "0")]
    [InlineData("long", "0")]
    [InlineData("short", "0")]
    [InlineData("byte", "0")]
    [InlineData("sbyte", "0")]
    [InlineData("uint", "0")]
    [InlineData("ulong", "0")]
    [InlineData("ushort", "0")]
    [InlineData("nint", "0")]
    [InlineData("nuint", "0")]
    [InlineData("bool", "false")]
    [InlineData("double", "0.0")]
    [InlineData("double", "0d")]
    [InlineData("double", "0.0d")]
    [InlineData("float", "0f")]
    [InlineData("float", "0.0f")]
    [InlineData("decimal", "0m")]
    [InlineData("decimal", "0.0m")]
    [InlineData("char", "'\\0'")]
    [InlineData("string?", "null")]
    [InlineData("object?", "null")]
    [InlineData("string", "null")]
    [InlineData("object", "null")]
    [InlineData("int", "default")]
    [InlineData("int", "default(int)")]
    [InlineData("bool", "default")]
    [InlineData("string?", "default")]
    public void Analyze_UnnecessaryDefaultInit_ReturnsDiagnostic(string type, string value)
    {
        string source = $$"""
            class Foo
            {
                {{type}} _x = {{value}};
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_unnecessary_initialization"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1805", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("int", "1")]
    [InlineData("bool", "true")]
    [InlineData("string?", "\"\"")]
    [InlineData("double", "1.0")]
    [InlineData("char", "'a'")]
    public void Analyze_NonDefaultValue_ReturnsNoDiagnostics(string type, string value)
    {
        string source = $$"""
            class Foo
            {
                {{type}} _x = {{value}};
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_unnecessary_initialization"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LocalVariable_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                void M()
                {
                    int x = 0;
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_unnecessary_initialization"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NoInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                int _x;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_unnecessary_initialization"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("int", "0")]
    [InlineData("byte", "0")]
    [InlineData("bool", "false")]
    [InlineData("string", "null")]
    public void Analyze_ConstField_ReturnsNoDiagnostics(string type, string value)
    {
        string source = $$"""
            class Foo
            {
                const {{type}} _x = {{value}};
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_unnecessary_initialization"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EnabledByDefault_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                int _x = 0;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_DisabledViaCustomKey_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                int _x = 0;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_unnecessary_initialization"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_DisabledViaDiagnosticKey_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                int _x = 0;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1805.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
