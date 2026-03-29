using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1217: Using static directives must be ordered alphabetically by full namespace.
/// </summary>
public sealed class StaticUsingDirectiveAlphabeticalOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_using_directive_ordering";

    public string RuleId => "SA1217";

    public string Name => "StaticUsingDirectiveAlphabeticalOrder";

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
            CheckStaticAlphabeticalOrder(group.Static, ref diagnostics);
        });

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void CheckStaticAlphabeticalOrder(List<UsingDirectiveSyntax> staticUsings, ref List<LintDiagnostic>? diagnostics)
    {
        string? prev = null;

        foreach (UsingDirectiveSyntax u in staticUsings)
        {
            string name = UsingDirectiveHelper.GetUsingName(u);

            if (prev is not null && string.Compare(name, prev, StringComparison.Ordinal) < 0)
            {
                FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Static using directives must be sorted alphabetically; '{name}' must come before '{prev}'",
                        Severity = DefaultSeverity,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            prev = name;
        }
    }
}
