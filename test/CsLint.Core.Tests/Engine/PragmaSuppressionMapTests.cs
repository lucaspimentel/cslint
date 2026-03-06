using Cslint.Core.Engine;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Tests.Engine;

public class PragmaSuppressionMapTests
{
    private static PragmaSuppressionMap BuildMap(string source)
    {
        var root = (CSharpSyntaxNode)CSharpSyntaxTree.ParseText(source).GetRoot();
        return PragmaSuppressionMap.Build(root);
    }

    [Fact]
    public void DisableSingleRule_SuppressesToEof()
    {
        const string Source = """
            #pragma warning disable CSLINT001
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.HasSuppressions);
        Assert.True(map.IsSuppressed("CSLINT001", 1));
        Assert.True(map.IsSuppressed("CSLINT001", 2));
        Assert.True(map.IsSuppressed("CSLINT001", 100));
    }

    [Fact]
    public void DisableAndRestore_SuppressesOnlyWithinRange()
    {
        const string Source = """
            class Before { }
            #pragma warning disable CSLINT001
            class Inside { }
            #pragma warning restore CSLINT001
            class After { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.HasSuppressions);
        Assert.False(map.IsSuppressed("CSLINT001", 1));
        Assert.True(map.IsSuppressed("CSLINT001", 2));
        Assert.True(map.IsSuppressed("CSLINT001", 3));
        Assert.True(map.IsSuppressed("CSLINT001", 4));
        Assert.False(map.IsSuppressed("CSLINT001", 5));
    }

    [Fact]
    public void DisableMultipleRules_SuppressesBoth()
    {
        const string Source = """
            #pragma warning disable CSLINT001, CSLINT002
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("CSLINT001", 2));
        Assert.True(map.IsSuppressed("CSLINT002", 2));
        Assert.False(map.IsSuppressed("CSLINT003", 2));
    }

    [Fact]
    public void DisableAll_SuppressesAllRules()
    {
        const string Source = """
            #pragma warning disable
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.HasSuppressions);
        Assert.True(map.IsSuppressed("CSLINT001", 2));
        Assert.True(map.IsSuppressed("CSLINT999", 2));
    }

    [Fact]
    public void RestoreWithoutDisable_HasNoEffect()
    {
        const string Source = """
            #pragma warning restore CSLINT001
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.HasSuppressions);
        Assert.False(map.IsSuppressed("CSLINT001", 1));
        Assert.False(map.IsSuppressed("CSLINT001", 2));
    }

    [Fact]
    public void NestedDisableRestore_MultiplePairsForSameRule()
    {
        const string Source = """
            class Before { }
            #pragma warning disable CSLINT001
            class First { }
            #pragma warning restore CSLINT001
            class Between { }
            #pragma warning disable CSLINT001
            class Second { }
            #pragma warning restore CSLINT001
            class After { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.IsSuppressed("CSLINT001", 1));
        Assert.True(map.IsSuppressed("CSLINT001", 2));
        Assert.True(map.IsSuppressed("CSLINT001", 3));
        Assert.True(map.IsSuppressed("CSLINT001", 4));
        Assert.False(map.IsSuppressed("CSLINT001", 5));
        Assert.True(map.IsSuppressed("CSLINT001", 6));
        Assert.True(map.IsSuppressed("CSLINT001", 7));
        Assert.True(map.IsSuppressed("CSLINT001", 8));
        Assert.False(map.IsSuppressed("CSLINT001", 9));
    }

    [Fact]
    public void UnrelatedRuleNotSuppressed()
    {
        const string Source = """
            #pragma warning disable CSLINT001
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("CSLINT001", 2));
        Assert.False(map.IsSuppressed("CSLINT002", 2));
    }

    [Fact]
    public void IsSuppressed_IsCaseInsensitive()
    {
        const string Source = """
            #pragma warning disable CSLINT001
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("cslint001", 2));
        Assert.True(map.IsSuppressed("CsLint001", 2));
    }

    [Fact]
    public void NoPragmas_HasNoSuppressions()
    {
        const string Source = """
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.HasSuppressions);
        Assert.False(map.IsSuppressed("CSLINT001", 1));
    }
}
