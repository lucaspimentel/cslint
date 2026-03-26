using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ObsoleteWithoutMessageRuleTests
{
    private readonly ObsoleteWithoutMessageRule _rule = new();

    [Theory]
    [InlineData("[Obsolete]")]
    [InlineData("[System.Obsolete]")]
    [InlineData("[ObsoleteAttribute]")]
    public void Analyze_ObsoleteWithoutMessage_ReturnsDiagnostic(string attribute)
    {
        string source = $$"""
            using System;

            class C
            {
                {{attribute}}
                public void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1041", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_ObsoleteWithEmptyString_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                [Obsolete("")]
                public void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_ObsoleteWithMessage_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                [Obsolete("Use NewMethod instead")]
                public void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ObsoleteOnClass_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            [Obsolete]
            class C { }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_NoObsoleteAttribute_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public void M() { }
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
                [Obsolete]
                public void M() { }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1041.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
