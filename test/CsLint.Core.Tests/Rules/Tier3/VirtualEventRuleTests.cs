using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class VirtualEventRuleTests
{
    private readonly VirtualEventRule _rule = new();

    [Fact]
    public void Analyze_VirtualEventField_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                public virtual event EventHandler MyEvent;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1070", diagnostic.RuleId);
        Assert.Contains("MyEvent", diagnostic.Message);
    }

    [Fact]
    public void Analyze_NonVirtualEventField_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                public event EventHandler MyEvent;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_VirtualEventWithAccessors_NoDiagnostic()
    {
        // Event with explicit add/remove accessors — not an EventFieldDeclaration
        const string source = """
            using System;

            class C
            {
                private EventHandler _handler;

                public virtual event EventHandler MyEvent
                {
                    add { _handler += value; }
                    remove { _handler -= value; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                public virtual event EventHandler MyEvent;
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1070.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
