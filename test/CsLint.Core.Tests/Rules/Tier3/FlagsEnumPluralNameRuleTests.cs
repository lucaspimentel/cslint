using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class FlagsEnumPluralNameRuleTests
{
    private readonly FlagsEnumPluralNameRule _rule = new();

    [Fact]
    public void Analyze_FlagsSingularName_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            [Flags]
            enum Permission { Read = 1, Write = 2 }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1714", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_FlagsPluralName_NoDiagnostic()
    {
        const string source = """
            using System;

            [Flags]
            enum Permissions { Read = 1, Write = 2 }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NonFlagsEnum_NoDiagnostic()
    {
        const string source = "enum Color { Red, Green, Blue }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            using System;

            [Flags]
            enum Permission { Read = 1 }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1714.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
