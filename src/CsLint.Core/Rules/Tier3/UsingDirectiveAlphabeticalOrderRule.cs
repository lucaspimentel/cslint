using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1210: Using directives must be ordered alphabetically by namespace.
/// </summary>
public sealed class UsingDirectiveAlphabeticalOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_using_directive_ordering";

    public string RuleId => "SA1210";

    public string Name => "UsingDirectiveAlphabeticalOrder";

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
            CheckAlphabeticalOrder(group.Regular, ref diagnostics);
        });

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void CheckAlphabeticalOrder(List<UsingDirectiveSyntax> regular, ref List<LintDiagnostic>? diagnostics)
    {
        string? prevSystem = null;
        string? prevNonSystem = null;

        foreach (UsingDirectiveSyntax u in regular)
        {
            string name = UsingDirectiveHelper.GetUsingName(u);
            bool isSystem = UsingDirectiveHelper.IsSystemNamespace(name);

            if (isSystem)
            {
                if (prevSystem is not null && string.Compare(name, prevSystem, StringComparison.Ordinal) < 0)
                {
                    FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = $"Using directives must be sorted alphabetically; '{name}' must come before '{prevSystem}'",
                            Severity = DefaultSeverity,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }

                prevSystem = name;
            }
            else
            {
                if (prevNonSystem is not null && string.Compare(name, prevNonSystem, StringComparison.Ordinal) < 0)
                {
                    FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = $"Using directives must be sorted alphabetically; '{name}' must come before '{prevNonSystem}'",
                            Severity = DefaultSeverity,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }

                prevNonSystem = name;
            }
        }
    }
}
