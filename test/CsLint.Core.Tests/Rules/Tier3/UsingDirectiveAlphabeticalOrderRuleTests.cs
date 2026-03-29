using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class UsingDirectiveAlphabeticalOrderRuleTests
{
    private readonly UsingDirectiveAlphabeticalOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_using_directive_ordering"] = "true",
        });

    [Fact]
    public void Analyze_SystemGroupAlphabetical_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SystemGroupNotAlphabetical_ReturnsDiagnostic()
    {
        string source = """
            using System.Linq;
            using System.Collections.Generic;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1210", diagnostics[0].RuleId);
        Assert.Contains("alphabetically", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_NonSystemGroupAlphabetical_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Alpha.Beta;
            using Gamma.Delta;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NonSystemGroupNotAlphabetical_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using Gamma.Delta;
            using Alpha.Beta;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1210", diagnostics[0].RuleId);
        Assert.Contains("alphabetically", diagnostics[0].Message);
    }
}
