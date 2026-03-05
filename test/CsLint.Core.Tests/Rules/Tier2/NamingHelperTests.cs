using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class NamingHelperTests
{
    [Theory]
    [InlineData("FooBar", true)]
    [InlineData("Foo", true)]
    [InlineData("F", true)]
    [InlineData("fooBar", false)]
    [InlineData("foo", false)]
    [InlineData("_foo", false)]
    [InlineData("", false)]
    public void IsPascalCase(string name, bool expected) =>
        Assert.Equal(expected, NamingHelper.IsPascalCase(name));

    [Theory]
    [InlineData("fooBar", true)]
    [InlineData("foo", true)]
    [InlineData("f", true)]
    [InlineData("FooBar", false)]
    [InlineData("_foo", false)]
    [InlineData("", false)]
    public void IsCamelCase(string name, bool expected) =>
        Assert.Equal(expected, NamingHelper.IsCamelCase(name));

    [Theory]
    [InlineData("_fooBar", true)]
    [InlineData("_foo", true)]
    [InlineData("_f", true)]
    [InlineData("_Foo", false)]
    [InlineData("foo", false)]
    [InlineData("_", false)]
    [InlineData("", false)]
    public void IsUnderscoreCamelCase(string name, bool expected) =>
        Assert.Equal(expected, NamingHelper.IsUnderscoreCamelCase(name));

    [Theory]
    [InlineData("FOO_BAR", true)]
    [InlineData("FOO", true)]
    [InlineData("MAX_VALUE", true)]
    [InlineData("Foo", false)]
    [InlineData("fooBar", false)]
    [InlineData("", false)]
    public void IsUpperCase(string name, bool expected) =>
        Assert.Equal(expected, NamingHelper.IsUpperCase(name));

    [Theory]
    [InlineData("IFoo", "I", true)]
    [InlineData("Foo", "I", false)]
    [InlineData("I", "I", false)]
    public void HasPrefix(string name, string prefix, bool expected) =>
        Assert.Equal(expected, NamingHelper.HasPrefix(name, prefix));
}
