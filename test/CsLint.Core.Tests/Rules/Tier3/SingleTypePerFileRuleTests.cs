using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SingleTypePerFileRuleTests
{
    private readonly SingleTypePerFileRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_single_type_per_file"] = "true",
        });

    [Fact]
    public void Analyze_TwoClasses_ReturnsDiagnostic()
    {
        string source = """
            class A { }
            class B { }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT252", diagnostics[0].RuleId);
        Assert.Contains("B", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ThreeTypes_ReturnsTwoDiagnostics()
    {
        string source = """
            class A { }
            struct B { }
            interface IC { }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void Analyze_TwoClassesInNamespace_ReturnsDiagnostic()
    {
        string source = """
            namespace Foo
            {
                class A { }
                class B { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_SingleClass_ReturnsNoDiagnostics()
    {
        string source = "class A { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NestedClass_ReturnsNoDiagnostics()
    {
        string source = """
            class A
            {
                class B { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class A { } class B { }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_single_type_per_file"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
