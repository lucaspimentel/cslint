using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EnumStorageShouldBeInt32RuleTests
{
    private readonly EnumStorageShouldBeInt32Rule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1028.severity"] = "warning",
    });

    [Theory]
    [InlineData("enum E : byte { A }")]
    [InlineData("enum E : long { A }")]
    [InlineData("enum E : short { A }")]
    [InlineData("enum E : ushort { A }")]
    [InlineData("enum E : uint { A }")]
    [InlineData("enum E : ulong { A }")]
    [InlineData("enum E : sbyte { A }")]
    public void Analyze_NonInt32BaseType_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1028", diagnostic.RuleId);
    }

    [Theory]
    [InlineData("enum E { A }")]
    [InlineData("enum E : int { A }")]
    public void Analyze_Int32OrDefault_NoDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "enum E : byte { A }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1028.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
