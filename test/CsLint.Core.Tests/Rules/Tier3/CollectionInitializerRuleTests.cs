using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class CollectionInitializerRuleTests
{
    private readonly CollectionInitializerRule _rule = new();

    [Fact]
    public void Analyze_NewFollowedByAddCalls_ReturnsDiagnostic()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                void M()
                {
                    var list = new List<int>();
                    list.Add(1);
                    list.Add(2);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_collection_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT216", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NewWithExistingInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                void M()
                {
                    var list = new List<int> { 1, 2 };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_collection_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NewFollowedByNonAddMethod_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                void M()
                {
                    var list = new List<int>();
                    list.Clear();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_collection_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AddCallOnDifferentVariable_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                void M()
                {
                    var list = new List<int>();
                    Console.WriteLine(list);
                    other.Add(1);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_collection_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                void M()
                {
                    var list = new List<int>();
                    list.Add(1);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_collection_initializer"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                void M()
                {
                    var list = new List<int>();
                    list.Add(1);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_HashCodeWithAdd_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                public override int GetHashCode()
                {
                    var hashCode = new HashCode();
                    hashCode.Add(1);
                    hashCode.Add(2);
                    return hashCode.ToHashCode();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_collection_initializer"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
