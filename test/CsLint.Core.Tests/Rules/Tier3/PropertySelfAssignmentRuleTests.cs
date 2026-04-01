using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PropertySelfAssignmentRuleTests
{
    private readonly PropertySelfAssignmentRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA2245.severity"] = "warning",
    });

    [Theory]
    [InlineData("Name = Name;")]
    [InlineData("this.Name = this.Name;")]
    public void Analyze_SelfAssignment_ReturnsDiagnostic(string assignment)
    {
        string source = $$"""
            namespace N;

            class C
            {
                public string Name { get; set; }

                void M()
                {
                    {{assignment}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2245", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_DifferentProperties_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public string Name { get; set; }
                public string Other { get; set; }

                void M()
                {
                    Name = Other;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ObjectInitializer_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class Tags
            {
                public string Name { get; set; }
            }

            class C
            {
                public string Name { get; set; }

                void M()
                {
                    var t = new Tags { Name = Name };
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AssignmentInPropertySetter_NoDiagnostic()
    {
        // CA2011 handles this case
        const string source = """
            namespace N;

            class C
            {
                public string Name
                {
                    get => "";
                    set { Name = value; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public string Name { get; set; }
                void M() { Name = Name; }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2245.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
