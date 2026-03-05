namespace Cslint.Core.Rules.Tier2;

internal static class NamingHelper
{
    public static bool IsPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return char.IsUpper(name[0]) && !ContainsConsecutiveUnderscores(name);
    }

    public static bool IsCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return char.IsLower(name[0]) && !ContainsConsecutiveUnderscores(name);
    }

    public static bool IsUnderscoreCamelCase(string name)
    {
        if (name is not { Length: >= 2 })
        {
            return false;
        }

        return name[0] == '_' && char.IsLower(name[1]);
    }

    public static bool IsUpperCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        foreach (char c in name)
        {
            if (char.IsLetter(c) && !char.IsUpper(c))
            {
                return false;
            }
        }

        return true;
    }

    public static bool HasPrefix(string name, string prefix) =>
        name.Length > prefix.Length && name.StartsWith(prefix, StringComparison.Ordinal);

    private static bool ContainsConsecutiveUnderscores(string name)
    {
        for (int i = 1; i < name.Length; i++)
        {
            if (name[i] == '_' && name[i - 1] == '_')
            {
                return true;
            }
        }

        return false;
    }
}
