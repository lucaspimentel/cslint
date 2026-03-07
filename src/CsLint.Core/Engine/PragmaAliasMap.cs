namespace Cslint.Core.Engine;

internal static class PragmaAliasMap
{
    private static readonly Dictionary<string, string[]> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SA1313"] = ["CSLINT103"],
            ["SA1306"] = ["CSLINT104"],
            ["SA1300"] = ["CSLINT102"],
            ["IDE1006"] = ["CSLINT102", "CSLINT103", "CSLINT104"],
        };

    public static bool TryGetMappedIds(string id, out string[] cslintIds) =>
        Aliases.TryGetValue(id, out cslintIds!);
}
