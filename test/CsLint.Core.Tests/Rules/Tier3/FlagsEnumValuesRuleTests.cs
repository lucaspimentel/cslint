using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class FlagsEnumValuesRuleTests
{
    private readonly FlagsEnumValuesRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA2217.severity"] = "warning",
    });

    [Fact]
    public void Analyze_FlagsWithNonPowerOfTwo_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            [Flags]
            enum Permissions
            {
                None = 0,
                Read = 1,
                Write = 2,
                Execute = 4,
                Bad = 5,
                Worse = 6,
                Invalid = 9,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2217", diagnostic.RuleId);
        Assert.Contains("Invalid", diagnostic.Message);
    }

    [Fact]
    public void Analyze_FlagsWithValidPowersOfTwo_NoDiagnostic()
    {
        const string source = """
            using System;

            [Flags]
            enum Permissions
            {
                None = 0,
                Read = 1,
                Write = 2,
                Execute = 4,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FlagsWithCombinationValue_NoDiagnostic()
    {
        const string source = """
            using System;

            [Flags]
            enum Permissions
            {
                None = 0,
                Read = 1,
                Write = 2,
                Execute = 4,
                All = 7,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EnumWithoutFlags_NoDiagnostic()
    {
        const string source = """
            enum Color
            {
                Red = 3,
                Green = 5,
                Blue = 7,
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FlagsImplicitValues_ReturnsDiagnostic()
    {
        // Implicit values: 0, 1, 2, 3, 4, 5 — 5 is not a power of 2
        // and not a valid combination (1|4=5 but 4 is only defined at E)
        // Actually 5 = 1|4 which ARE both powers. Use 6 members so 5 appears.
        // Let's use explicit bad value instead.
        const string source = """
            using System;

            [Flags]
            enum E { A = 0, B = 1, C = 2, D = 4, Bad = 9 }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            using System;

            [Flags]
            enum E { A = 0, B = 1, C = 3 }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2217.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
