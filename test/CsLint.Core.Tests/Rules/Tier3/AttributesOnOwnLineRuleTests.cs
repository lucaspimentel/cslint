using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class AttributesOnOwnLineRuleTests
{
    private readonly AttributesOnOwnLineRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_attributes_on_own_line"] = "true",
        });

    [Theory]
    [InlineData("[Serializable] class C { }")]
    [InlineData("[Obsolete] void M() { }")]
    [InlineData("[Obsolete] int X { get; }")]
    public void Analyze_AttributeOnSameLineAsDeclaration_ReturnsDiagnostic(string code)
    {
        string source = $"using System; class Outer {{ {code} }}";

        // For the class case, adjust the source
        if (code.Contains("class C"))
        {
            source = $"using System; {code}";
        }

        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT245", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_AttributeOnOwnLine_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            [Serializable]
            class C { }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleAttributesOnOwnLines_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            [Serializable]
            [Obsolete]
            class C { }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MethodAttributeOnOwnLine_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class C
            {
                [Obsolete]
                void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NoAttributes_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M() { }
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
        string source = "using System; [Serializable] class C { }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_attributes_on_own_line"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
