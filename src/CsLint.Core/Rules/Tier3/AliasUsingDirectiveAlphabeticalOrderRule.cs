using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1211: Using alias directives must be ordered alphabetically by alias name.
/// </summary>
public sealed class AliasUsingDirectiveAlphabeticalOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_using_directive_ordering";

    public string RuleId => "SA1211";

    public string Name => "AliasUsingDirectiveAlphabeticalOrder";

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
            CheckAliasAlphabeticalOrder(group.Alias, ref diagnostics);
        });

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void CheckAliasAlphabeticalOrder(List<UsingDirectiveSyntax> aliasUsings, ref List<LintDiagnostic>? diagnostics)
    {
        string? prev = null;

        foreach (UsingDirectiveSyntax u in aliasUsings)
        {
            string aliasName = u.Alias!.Name.ToString();

            if (prev is not null && string.Compare(aliasName, prev, StringComparison.Ordinal) < 0)
            {
                FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Using alias directives must be sorted alphabetically by alias name; '{aliasName}' must come before '{prev}'",
                        Severity = DefaultSeverity,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            prev = aliasName;
        }
    }
}
