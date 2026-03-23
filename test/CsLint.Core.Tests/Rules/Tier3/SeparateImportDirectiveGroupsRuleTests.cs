using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SeparateImportDirectiveGroupsRuleTests
{
    private readonly SeparateImportDirectiveGroupsRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["dotnet_separate_import_directive_groups"] = "true",
        });

    [Fact]
    public void Analyze_GroupsSeparatedByBlankLine_ReturnsNoDiagnostics()
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
    public void Analyze_GroupsNotSeparated_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using System.Collections.Generic;
            using Newtonsoft.Json;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT278", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_StaticGroupNotSeparated_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_StaticGroupSeparated_ReturnsNoDiagnostics()
    {
        string source = """
            using System;

            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AllSameGroup_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Linq;
            using System.Threading;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ThreeGroupsAllSeparated_ReturnsNoDiagnostics()
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
    public void Analyze_ThreeGroupsOneMissingSeparator_ReturnsDiagnostic()
    {
        string source = """
            using System;

            using Xunit;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_InsideNamespace_ChecksUsings()
    {
        string source = """
            namespace Foo
            {
                using System;
                using Xunit;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigDisabled_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Xunit;
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_separate_import_directive_groups"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Xunit;
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
