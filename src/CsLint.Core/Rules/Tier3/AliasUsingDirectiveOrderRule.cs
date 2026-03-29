using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1209: Using alias directives must be placed after other using directives.
/// </summary>
public sealed class AliasUsingDirectiveOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_using_directive_ordering";

    public string RuleId => "SA1209";

    public string Name => "AliasUsingDirectiveOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        UsingDirectiveHelper.ForEachUsingGroup(context.Root, usings =>
        {
            ClassifiedUsingGroup group = UsingDirectiveHelper.ClassifyGroup(usings);
            CheckAliasAfterAll(group.Regular, group.Static, group.Alias, ref diagnostics);
        });

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void CheckAliasAfterAll(
        List<UsingDirectiveSyntax> regular,
        List<UsingDirectiveSyntax> staticUsings,
        List<UsingDirectiveSyntax> aliasUsings,
        ref List<LintDiagnostic>? diagnostics)
    {
        if (aliasUsings.Count == 0)
        {
            return;
        }

        int lastRegularLine = regular.Count > 0
            ? regular[^1].GetLocation().GetLineSpan().StartLinePosition.Line
            : -1;

        int lastStaticLine = staticUsings.Count > 0
            ? staticUsings[^1].GetLocation().GetLineSpan().StartLinePosition.Line
            : -1;

        int lastNonAliasLine = Math.Max(lastRegularLine, lastStaticLine);
        int firstAliasLine = aliasUsings[0].GetLocation().GetLineSpan().StartLinePosition.Line;

        if (firstAliasLine < lastNonAliasLine)
        {
            foreach (UsingDirectiveSyntax a in aliasUsings)
            {
                int line = a.GetLocation().GetLineSpan().StartLinePosition.Line;

                if (line < lastNonAliasLine)
                {
                    FileLinePositionSpan span = a.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Alias using directives must appear after all other using directives",
                            Severity = DefaultSeverity,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }
            }
        }
    }
}
