using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class DuplicateIndexedElementInitRuleTests
{
    private readonly DuplicateIndexedElementInitRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA2244.severity"] = "warning",
    });

    [Fact]
    public void Analyze_DuplicateIndex_ReturnsDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;
            namespace N;

            class C
            {
                void M()
                {
                    var d = new Dictionary<string, int>
                    {
                        ["a"] = 1,
                        ["b"] = 2,
                        ["a"] = 3,
                    };
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2244", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_UniqueIndices_NoDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;
            namespace N;

            class C
            {
                void M()
                {
                    var d = new Dictionary<string, int>
                    {
                        ["a"] = 1,
                        ["b"] = 2,
                        ["c"] = 3,
                    };
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
            using System.Collections.Generic;
            namespace N;

            class C
            {
                void M()
                {
                    var d = new Dictionary<string, int>
                    {
                        ["a"] = 1,
                        ["a"] = 2,
                    };
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2244.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
