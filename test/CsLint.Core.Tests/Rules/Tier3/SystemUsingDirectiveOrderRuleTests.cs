using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SystemUsingDirectiveOrderRuleTests
{
    private readonly SystemUsingDirectiveOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_using_directive_ordering"] = "true",
        });

    [Fact]
    public void Analyze_SystemBeforeNonSystem_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Collections.Generic;
            using Microsoft.Extensions;
            using Newtonsoft.Json;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SystemAfterNonSystem_ReturnsDiagnostic()
    {
        string source = """
            using Newtonsoft.Json;
            using System;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1208", diagnostics[0].RuleId);
        Assert.Contains("System", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            using Newtonsoft.Json;
            using System;
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_using_directive_ordering"] = configValue;
        }

        RuleContext context = TestHelper.CreateContext(source, new LintConfiguration(settings));

        Assert.Empty(_rule.Analyze(context));
    }
}
