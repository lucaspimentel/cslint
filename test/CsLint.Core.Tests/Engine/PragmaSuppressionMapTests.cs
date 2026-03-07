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

    [Fact]
    public void SA1313_SuppressesCslint103()
    {
        const string Source = """
            #pragma warning disable SA1313
            void M(int camelCase) { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("CSLINT103", 2));
        Assert.False(map.IsSuppressed("CSLINT102", 2));
        Assert.False(map.IsSuppressed("CSLINT104", 2));
    }

    [Fact]
    public void IDE1006_SuppressesMultipleCslintRules()
    {
        const string Source = """
            #pragma warning disable IDE1006
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("CSLINT102", 2));
        Assert.True(map.IsSuppressed("CSLINT103", 2));
        Assert.True(map.IsSuppressed("CSLINT104", 2));
        Assert.False(map.IsSuppressed("CSLINT101", 2));
    }

    [Fact]
    public void SA1300_DisableRestore_SuppressesOnlyInRange()
    {
        const string Source = """
            class Before { }
            #pragma warning disable SA1300
            class Inside { }
            #pragma warning restore SA1300
            class After { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.IsSuppressed("CSLINT102", 1));
        Assert.True(map.IsSuppressed("CSLINT102", 2));
        Assert.True(map.IsSuppressed("CSLINT102", 3));
        Assert.True(map.IsSuppressed("CSLINT102", 4));
        Assert.False(map.IsSuppressed("CSLINT102", 5));
    }

    [Fact]
    public void UnmappedThirdPartyId_IsIgnored()
    {
        const string Source = """
            #pragma warning disable CA1000
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.IsSuppressed("CSLINT102", 2));
        Assert.False(map.IsSuppressed("CSLINT103", 2));
        Assert.False(map.IsSuppressed("CSLINT104", 2));
    }

    [Theory]
    [InlineData("IDE0007", "CSLINT200")]
    [InlineData("IDE0008", "CSLINT200")]
    [InlineData("IDE0011", "CSLINT202")]
    [InlineData("IDE0021", "CSLINT201")]
    [InlineData("IDE0036", "CSLINT205")]
    [InlineData("IDE0040", "CSLINT206")]
    [InlineData("IDE0049", "CSLINT208")]
    [InlineData("IDE0065", "CSLINT207")]
    [InlineData("IDE0160", "CSLINT203")]
    [InlineData("IDE0003", "CSLINT204")]
    [InlineData("IDE0019", "CSLINT209")]
    [InlineData("IDE0029", "CSLINT210")]
    [InlineData("IDE0041", "CSLINT210")]
    public void IdeAlias_SuppressesMappedCslintRule(string ideId, string expectedCslintId)
    {
        string source = $"#pragma warning disable {ideId}\nclass Foo {{ }}\n";

        PragmaSuppressionMap map = BuildMap(source);

        Assert.True(map.IsSuppressed(expectedCslintId, 2));
    }
}
