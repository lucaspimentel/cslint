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
            #pragma warning disable SA1028
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.HasSuppressions);
        Assert.True(map.IsSuppressed("SA1028", 1));
        Assert.True(map.IsSuppressed("SA1028", 2));
        Assert.True(map.IsSuppressed("SA1028", 100));
    }

    [Fact]
    public void DisableAndRestore_SuppressesOnlyWithinRange()
    {
        const string Source = """
            class Before { }
            #pragma warning disable SA1028
            class Inside { }
            #pragma warning restore SA1028
            class After { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.HasSuppressions);
        Assert.False(map.IsSuppressed("SA1028", 1));
        Assert.True(map.IsSuppressed("SA1028", 2));
        Assert.True(map.IsSuppressed("SA1028", 3));
        Assert.True(map.IsSuppressed("SA1028", 4));
        Assert.False(map.IsSuppressed("SA1028", 5));
    }

    [Fact]
    public void DisableMultipleRules_SuppressesBoth()
    {
        const string Source = """
            #pragma warning disable SA1028, SA1027
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("SA1028", 2));
        Assert.True(map.IsSuppressed("SA1027", 2));
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
        Assert.True(map.IsSuppressed("SA1028", 2));
        Assert.True(map.IsSuppressed("CSLINT999", 2));
    }

    [Fact]
    public void RestoreWithoutDisable_HasNoEffect()
    {
        const string Source = """
            #pragma warning restore SA1028
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.HasSuppressions);
        Assert.False(map.IsSuppressed("SA1028", 1));
        Assert.False(map.IsSuppressed("SA1028", 2));
    }

    [Fact]
    public void NestedDisableRestore_MultiplePairsForSameRule()
    {
        const string Source = """
            class Before { }
            #pragma warning disable SA1028
            class First { }
            #pragma warning restore SA1028
            class Between { }
            #pragma warning disable SA1028
            class Second { }
            #pragma warning restore SA1028
            class After { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.IsSuppressed("SA1028", 1));
        Assert.True(map.IsSuppressed("SA1028", 2));
        Assert.True(map.IsSuppressed("SA1028", 3));
        Assert.True(map.IsSuppressed("SA1028", 4));
        Assert.False(map.IsSuppressed("SA1028", 5));
        Assert.True(map.IsSuppressed("SA1028", 6));
        Assert.True(map.IsSuppressed("SA1028", 7));
        Assert.True(map.IsSuppressed("SA1028", 8));
        Assert.False(map.IsSuppressed("SA1028", 9));
    }

    [Fact]
    public void UnrelatedRuleNotSuppressed()
    {
        const string Source = """
            #pragma warning disable SA1028
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("SA1028", 2));
        Assert.False(map.IsSuppressed("SA1027", 2));
    }

    [Fact]
    public void IsSuppressed_IsCaseInsensitive()
    {
        const string Source = """
            #pragma warning disable SA1028
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("sa1028", 2));
        Assert.True(map.IsSuppressed("Sa1028", 2));
    }

    [Fact]
    public void NoPragmas_HasNoSuppressions()
    {
        const string Source = """
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.HasSuppressions);
        Assert.False(map.IsSuppressed("SA1028", 1));
    }

    [Fact]
    public void SA1313_SuppressesParameterNaming()
    {
        const string Source = """
            #pragma warning disable SA1313
            void M(int camelCase) { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("SA1313", 2));
        Assert.False(map.IsSuppressed("SA1300", 2));
        Assert.False(map.IsSuppressed("SA1306", 2));
    }

    [Fact]
    public void IDE1006_SuppressesMultipleNamingRules()
    {
        const string Source = """
            #pragma warning disable IDE1006
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("SA1300", 2));
        Assert.True(map.IsSuppressed("SA1312", 2));
        Assert.True(map.IsSuppressed("SA1313", 2));
        Assert.True(map.IsSuppressed("SA1306", 2));
        Assert.False(map.IsSuppressed("SA1302", 2));
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

        Assert.False(map.IsSuppressed("SA1300", 1));
        Assert.True(map.IsSuppressed("SA1300", 2));
        Assert.True(map.IsSuppressed("SA1300", 3));
        Assert.True(map.IsSuppressed("SA1300", 4));
        Assert.False(map.IsSuppressed("SA1300", 5));
    }

    [Fact]
    public void UnmappedThirdPartyId_IsIgnored()
    {
        const string Source = """
            #pragma warning disable CA1000
            class Foo { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.False(map.IsSuppressed("SA1300", 2));
        Assert.False(map.IsSuppressed("CSLINT103", 2));
        Assert.False(map.IsSuppressed("CSLINT104", 2));
    }

    [Fact]
    public void SA1302_SuppressesCslint101()
    {
        const string Source = """
            #pragma warning disable SA1302
            interface MappedDiagnosticsProxy { }
            """;

        PragmaSuppressionMap map = BuildMap(Source);

        Assert.True(map.IsSuppressed("SA1302", 2));
    }

    [Theory]
    [InlineData("IDE0007", "IDE0007")]
    [InlineData("IDE0008", "IDE0008")]
    [InlineData("IDE0011", "IDE0011")]
    [InlineData("IDE0021", "IDE0021")]
    [InlineData("IDE0029", "IDE0029")]
    [InlineData("IDE0036", "IDE0036")]
    [InlineData("IDE0040", "IDE0040")]
    [InlineData("IDE0049", "IDE0049")]
    [InlineData("IDE0065", "IDE0065")]
    [InlineData("IDE0160", "CSLINT203")]
    [InlineData("IDE0003", "SA1101")]
    [InlineData("IDE0019", "IDE0019")]
    public void IdeAlias_SuppressesMappedCslintRule(string ideId, string expectedCslintId)
    {
        string source = $"#pragma warning disable {ideId}\nclass Foo {{ }}\n";

        PragmaSuppressionMap map = BuildMap(source);

        Assert.True(map.IsSuppressed(expectedCslintId, 2));
    }
}
