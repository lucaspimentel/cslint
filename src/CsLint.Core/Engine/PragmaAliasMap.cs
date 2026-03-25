namespace Cslint.Core.Engine;

internal static class PragmaAliasMap
{
    private static readonly Dictionary<string, string[]> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // StyleCop
            ["SA1027"] = ["CSLINT002"],
            ["SA1028"] = ["CSLINT001"],
            ["SA1000"] = ["CSLINT254"],
            ["SA1001"] = ["CSLINT255"],
            ["SA1002"] = ["CSLINT256"],
            ["SA1003"] = ["CSLINT257"],
            ["SA1005"] = ["CSLINT258"],
            ["SA1008"] = ["CSLINT259"],
            ["SA1009"] = ["CSLINT259"],
            ["SA1012"] = ["CSLINT260"],
            ["SA1013"] = ["CSLINT260"],
            ["SA1024"] = ["CSLINT261"],
            ["SA1025"] = ["CSLINT262"],
            ["SA1101"] = ["CSLINT204"],
            ["SA1212"] = ["CSLINT263"],
            ["SA1213"] = ["CSLINT263"],
            ["SA1214"] = ["CSLINT264"],
            ["SA1203"] = ["CSLINT265"],
            ["SA1204"] = ["CSLINT266"],
            ["SA1202"] = ["CSLINT267"],
            ["SA1201"] = ["CSLINT268"],
            ["SA1208"] = ["CSLINT269"],
            ["SA1209"] = ["CSLINT269"],
            ["SA1210"] = ["CSLINT269"],
            ["SA1211"] = ["CSLINT269"],
            ["SA1216"] = ["CSLINT269"],
            ["SA1217"] = ["CSLINT269"],
            ["SA1121"] = ["CSLINT208"],
            ["SA1124"] = ["CSLINT006"],
            ["SA1206"] = ["CSLINT205"],
            ["SA1300"] = ["CSLINT102"],
            ["SA1302"] = ["CSLINT101"],
            ["SA1303"] = ["CSLINT105"],
            ["SA1304"] = ["CSLINT104"],
            ["SA1306"] = ["CSLINT104"],
            ["SA1307"] = ["CSLINT104"],
            ["SA1311"] = ["CSLINT104"],
            ["SA1314"] = ["CSLINT106"],
            ["SA1312"] = ["CSLINT103"],
            ["SA1313"] = ["CSLINT103"],
            ["SA1400"] = ["CSLINT206"],
            ["SA1500"] = ["CSLINT202"],
            ["SA1503"] = ["CSLINT228"],
            ["SA1507"] = ["CSLINT008"],
            ["SA1106"] = ["CSLINT240"],
            ["SA1107"] = ["CSLINT241"],
            ["SA1131"] = ["CSLINT242"],
            ["SA1132"] = ["CSLINT243"],
            ["SA1133"] = ["CSLINT244"],
            ["SA1134"] = ["CSLINT245"],
            ["SA1136"] = ["CSLINT246"],
            ["SA1505"] = ["CSLINT247"],
            ["SA1508"] = ["CSLINT248"],
            ["SA1509"] = ["CSLINT249"],
            ["SA1516"] = ["CSLINT250"],
            ["SA1401"] = ["CSLINT251"],
            ["SA1402"] = ["CSLINT252"],
            ["SA1412"] = ["CSLINT010"],
            ["SA1413"] = ["CSLINT253"],
            ["SA1517"] = ["CSLINT009"],
            ["SA1518"] = ["CSLINT004"],

            // Microsoft IDE
            ["IDE0003"] = ["CSLINT204"],
            ["IDE0007"] = ["CSLINT200"],
            ["IDE0008"] = ["CSLINT200"],
            ["IDE0009"] = ["CSLINT204"],
            ["IDE0011"] = ["CSLINT202"],
            ["IDE0016"] = ["CSLINT210"],
            ["IDE0018"] = ["CSLINT272"],
            ["IDE0019"] = ["CSLINT270"],
            ["IDE0020"] = ["CSLINT209"],
            ["IDE0038"] = ["CSLINT209"],
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
            ["IDE0039"] = ["CSLINT276"],
            ["IDE0062"] = ["IDE0062"],
            ["IDE0320"] = ["IDE0320"],
            ["IDE0150"] = ["IDE0150"],
            ["IDE0330"] = ["IDE0330"],
            ["IDE0350"] = ["IDE0350"],
            ["IDE0040"] = ["CSLINT206"],
            ["IDE0041"] = ["CSLINT210"],
            ["IDE0045"] = ["CSLINT274"],
            ["IDE0046"] = ["CSLINT275"],
            ["IDE0049"] = ["CSLINT208"],
            ["IDE0065"] = ["CSLINT207"],
            ["IDE0066"] = ["CSLINT273"],
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
            ["IDE1005"] = ["CSLINT271"],
            ["IDE1006"] = ["CSLINT102", "CSLINT103", "CSLINT104"],
            ["IDE2000"] = ["CSLINT008"],
            ["IDE2001"] = ["CSLINT228"],
            ["IDE2002"] = ["CSLINT229"],
            ["IDE2003"] = ["CSLINT230"],
            ["IDE2004"] = ["CSLINT231"],
            ["IDE2005"] = ["CSLINT232"],
            ["IDE2006"] = ["CSLINT233"],
            ["IDE0004"] = ["CSLINT306"],
            ["IDE0005"] = ["CSLINT300"],
            ["IDE0037"] = ["CSLINT234"],
            ["IDE0051"] = ["CSLINT308"],
            ["IDE0052"] = ["CSLINT308"],
            ["IDE0075"] = ["CSLINT235"],
            ["IDE0170"] = ["CSLINT236"],

            // C# compiler
            ["CS0162"] = ["CSLINT302"],
            ["CS0169"] = ["CSLINT308"],
            ["CS0219"] = ["CSLINT301"],
            ["CS0414"] = ["CSLINT308"],
            ["CS1717"] = ["CSLINT304"],

            // Microsoft CA (code quality)
            ["CA1069"] = ["CSLINT303"],
            ["CA1821"] = ["CSLINT237"],
            ["CA1805"] = ["CSLINT238"],
            ["CA1852"] = ["CSLINT239"],
        };

    public static bool TryGetMappedIds(string id, out string[] cslintIds) =>
        Aliases.TryGetValue(id, out cslintIds!);

    internal static IReadOnlyDictionary<string, List<string>> GetAliasesByCslintId()
    {
        var reverse = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach ((string alias, string[] cslintIds) in Aliases)
        {
            foreach (string cslintId in cslintIds)
            {
                if (!reverse.TryGetValue(cslintId, out List<string>? list))
                {
                    list = [];
                    reverse[cslintId] = list;
                }

                list.Add(alias);
            }
        }

        return reverse;
    }
}
