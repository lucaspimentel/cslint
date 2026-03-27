using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NoCombinedFieldDeclarationsRuleTests
{
    private readonly NoCombinedFieldDeclarationsRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_combined_field_declarations"] = "true",
        });

    [Theory]
    [InlineData("int x, y;")]
    [InlineData("string a, b, c;")]
    [InlineData("private int x, y;")]
    public void Analyze_CombinedFieldDeclaration_ReturnsDiagnostic(string field)
    {
        string source = $"class C {{ {field} }}";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1132", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("int x;")]
    [InlineData("private string name;")]
    [InlineData("readonly int value;")]
    public void Analyze_SingleFieldDeclaration_ReturnsNoDiagnostics(string field)
    {
        string source = $"class C {{ {field} }}";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = "class C { int x, y; }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_combined_field_declarations"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
