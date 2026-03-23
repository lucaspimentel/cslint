using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SortSystemDirectivesFirstRuleTests
{
    private readonly SortSystemDirectivesFirstRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["dotnet_sort_system_directives_first"] = "true",
        });

    [Fact]
    public void Analyze_SystemBeforeNonSystem_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Collections.Generic;
            using Newtonsoft.Json;
            using Xunit;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NonSystemBeforeSystem_ReturnsDiagnostic()
    {
        string source = """
            using Newtonsoft.Json;
            using System;
            using System.Collections.Generic;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT277", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_MixedWithStatic_IgnoresStatic()
    {
        string source = """
            using System;
            using Xunit;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_OnlySystem_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Linq;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_OnlyNonSystem_ReturnsNoDiagnostics()
    {
        string source = """
            using Newtonsoft.Json;
            using Xunit;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_InsideNamespace_ChecksUsings()
    {
        string source = """
            namespace Foo
            {
                using Xunit;
                using System;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigDisabled_ReturnsNoDiagnostics()
    {
        string source = """
            using Newtonsoft.Json;
            using System;
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_sort_system_directives_first"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            using Newtonsoft.Json;
            using System;
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
