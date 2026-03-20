using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class FieldsMustBePrivateRuleTests
{
    private readonly FieldsMustBePrivateRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_fields_must_be_private"] = "true",
        });

    [Theory]
    [InlineData("public int x;")]
    [InlineData("internal int x;")]
    [InlineData("protected int x;")]
    [InlineData("protected internal int x;")]
    public void Analyze_NonPrivateField_ReturnsDiagnostic(string field)
    {
        string source = $"class C {{ {field} }}";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT251", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("private int x;")]
    [InlineData("int x;")]
    [InlineData("public const int X = 1;")]
    [InlineData("public static readonly int X = 1;")]
    [InlineData("internal const string Y = \"test\";")]
    public void Analyze_PrivateOrExemptField_ReturnsNoDiagnostics(string field)
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
        string source = "class C { public int x; }";
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_fields_must_be_private"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
