namespace Cslint.Core.Engine;

internal static class PragmaAliasMap
{
    private static readonly Dictionary<string, string[]> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // StyleCop
            ["SA1300"] = ["CSLINT102"],
            ["SA1302"] = ["CSLINT101"],
            ["SA1306"] = ["CSLINT104"],
            ["SA1313"] = ["CSLINT103"],

            // Microsoft IDE
            ["IDE0003"] = ["CSLINT204"],
            ["IDE0007"] = ["CSLINT200"],
            ["IDE0008"] = ["CSLINT200"],
            ["IDE0009"] = ["CSLINT204"],
            ["IDE0011"] = ["CSLINT202"],
            ["IDE0019"] = ["CSLINT209"],
            ["IDE0020"] = ["CSLINT209"],
            ["IDE0021"] = ["CSLINT201"],
            ["IDE0022"] = ["CSLINT201"],
            ["IDE0023"] = ["CSLINT201"],
            ["IDE0024"] = ["CSLINT201"],
            ["IDE0025"] = ["CSLINT201"],
            ["IDE0026"] = ["CSLINT201"],
            ["IDE0027"] = ["CSLINT201"],
            ["IDE0029"] = ["CSLINT210"],
            ["IDE0030"] = ["CSLINT210"],
            ["IDE0031"] = ["CSLINT210"],
            ["IDE0036"] = ["CSLINT205"],
            ["IDE0040"] = ["CSLINT206"],
            ["IDE0041"] = ["CSLINT210"],
            ["IDE0049"] = ["CSLINT208"],
            ["IDE0065"] = ["CSLINT207"],
            ["IDE0066"] = ["CSLINT209"],
            ["IDE0160"] = ["CSLINT203"],
            ["IDE0161"] = ["CSLINT203"],
            ["IDE0034"] = ["CSLINT213"],
            ["IDE0063"] = ["CSLINT211"],
            ["IDE0073"] = ["CSLINT007"],
            ["IDE0090"] = ["CSLINT212"],
            ["IDE0017"] = ["CSLINT215"],
            ["IDE0028"] = ["CSLINT216"],
            ["IDE0053"] = ["CSLINT217"],
            ["IDE0054"] = ["CSLINT214"],
            ["IDE0061"] = ["CSLINT218"],
            ["IDE0074"] = ["CSLINT214"],
            ["IDE0078"] = ["CSLINT220"],
            ["IDE0083"] = ["CSLINT219"],
            ["IDE0056"] = ["CSLINT226"],
            ["IDE0057"] = ["CSLINT227"],
            ["IDE0071"] = ["CSLINT225"],
            ["IDE0180"] = ["CSLINT223"],
            ["IDE0230"] = ["CSLINT224"],
            ["IDE0290"] = ["CSLINT221"],
            ["IDE0300"] = ["CSLINT222"],
            ["IDE0301"] = ["CSLINT222"],
            ["IDE0302"] = ["CSLINT222"],
            ["IDE0303"] = ["CSLINT222"],
            ["IDE0304"] = ["CSLINT222"],
            ["IDE0305"] = ["CSLINT222"],
            ["IDE1006"] = ["CSLINT102", "CSLINT103", "CSLINT104"],
        };

    public static bool TryGetMappedIds(string id, out string[] cslintIds) =>
        Aliases.TryGetValue(id, out cslintIds!);
}
