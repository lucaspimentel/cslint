using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NoCombinedAttributesRuleTests
{
    private readonly NoCombinedAttributesRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_combined_attributes"] = "true",
        });

    [Theory]
    [InlineData("[Serializable, Obsolete]")]
    [InlineData("[Attr1, Attr2, Attr3]")]
    public void Analyze_CombinedAttributes_ReturnsDiagnostic(string attrs)
    {
        string source = $$"""
            using System;
            {{attrs}}
            class C { }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1133", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_SeparateAttributeLists_ReturnsNoDiagnostics()
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
    public void Analyze_SingleAttribute_ReturnsNoDiagnostics()
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

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            using System;
            [Serializable, Obsolete]
            class C { }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_combined_attributes"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
