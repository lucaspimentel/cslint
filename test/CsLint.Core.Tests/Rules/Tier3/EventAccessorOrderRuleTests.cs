using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EventAccessorOrderRuleTests
{
    private readonly EventAccessorOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string> { ["csharp_accessor_ordering"] = "true" });

    [Fact]
    public void EventAddBeforeRemove_NoDiagnostics()
    {
        string source = """
            using System;
            class Foo
            {
                event EventHandler E { add { } remove { } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void EventRemoveBeforeAdd_Flagged()
    {
        string source = """
            using System;
            class Foo
            {
                event EventHandler E { remove { } add { } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1213", diagnostics[0].RuleId);
        Assert.Contains("add", diagnostics[0].Message);
        Assert.Contains("remove", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void RuleDisabled_NoDiagnostics(string? configValue)
    {
        string source = """
            using System;
            class Foo
            {
                event EventHandler E { remove { } add { } }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_accessor_ordering"] = configValue;
        }

        RuleContext context = TestHelper.CreateContext(source, new LintConfiguration(settings));

        Assert.Empty(_rule.Analyze(context));
    }
}
