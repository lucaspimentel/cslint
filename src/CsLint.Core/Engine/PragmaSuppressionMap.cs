using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Engine;

public sealed class PragmaSuppressionMap
{
    // Empty string key means "all rules"
    private const string AllRulesKey = "";

    private readonly Dictionary<string, List<(int Start, int End)>> _suppressedRanges;

    public bool HasSuppressions => _suppressedRanges.Count > 0;

    private PragmaSuppressionMap(Dictionary<string, List<(int Start, int End)>> suppressedRanges)
    {
        _suppressedRanges = suppressedRanges;
    }

    public static PragmaSuppressionMap Build(CSharpSyntaxNode root)
    {
        var openRanges = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var suppressedRanges = new Dictionary<string, List<(int Start, int End)>>(StringComparer.OrdinalIgnoreCase);

        foreach (SyntaxTrivia trivia in root.DescendantTrivia())
        {
            if (!trivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia))
            {
                continue;
            }

            if (trivia.GetStructure() is not PragmaWarningDirectiveTriviaSyntax pragma)
            {
                continue;
            }

            // 0-indexed to 1-indexed
            int line = trivia.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            bool isDisable = pragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword);

            if (pragma.ErrorCodes.Count == 0)
            {
                // No specific IDs — applies to all rules
                if (isDisable)
                {
                    openRanges.TryAdd(AllRulesKey, line);
                }
                else
                {
                    CloseRange(openRanges, suppressedRanges, AllRulesKey, line);
                }
            }
            else
            {
                foreach (ExpressionSyntax errorCode in pragma.ErrorCodes)
                {
                    string id = errorCode.ToString();

                    if (isDisable)
                    {
                        openRanges.TryAdd(id, line);
                    }
                    else
                    {
                        CloseRange(openRanges, suppressedRanges, id, line);
                    }

                    // Also register mapped CsLint IDs for third-party rule aliases
                    if (PragmaAliasMap.TryGetMappedIds(id, out string[] mappedIds))
                    {
                        foreach (string mappedId in mappedIds)
                        {
                            if (isDisable)
                            {
                                openRanges.TryAdd(mappedId, line);
                            }
                            else
                            {
                                CloseRange(openRanges, suppressedRanges, mappedId, line);
                            }
                        }
                    }
                }
            }
        }

        // Close any remaining open ranges to EOF
        foreach (KeyValuePair<string, int> kvp in openRanges)
        {
            AddRange(suppressedRanges, kvp.Key, kvp.Value, int.MaxValue);
        }

        return new PragmaSuppressionMap(suppressedRanges);
    }

    public bool IsSuppressed(string ruleId, int line)
    {
        // Check rule-specific suppressions
        if (_suppressedRanges.TryGetValue(ruleId, out List<(int Start, int End)>? ranges))
        {
            for (int i = 0; i < ranges.Count; i++)
            {
                if (line >= ranges[i].Start && line <= ranges[i].End)
                {
                    return true;
                }
            }
        }

        // Check blanket suppressions (no specific ID)
        if (_suppressedRanges.TryGetValue(AllRulesKey, out List<(int Start, int End)>? allRanges))
        {
            for (int i = 0; i < allRanges.Count; i++)
            {
                if (line >= allRanges[i].Start && line <= allRanges[i].End)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void CloseRange(
        Dictionary<string, int> openRanges,
        Dictionary<string, List<(int Start, int End)>> suppressedRanges,
        string id,
        int endLine)
    {
        if (openRanges.Remove(id, out int startLine))
        {
            AddRange(suppressedRanges, id, startLine, endLine);
        }
    }

    private static void AddRange(
        Dictionary<string, List<(int Start, int End)>> suppressedRanges,
        string id,
        int start,
        int end)
    {
        if (!suppressedRanges.TryGetValue(id, out List<(int Start, int End)>? list))
        {
            list = [];
            suppressedRanges[id] = list;
        }

        list.Add((start, end));
    }
}
