using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1208: System using directives must be placed before other using directives.
/// </summary>
public sealed class SystemUsingDirectiveOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_using_directive_ordering";

    public string RuleId => "SA1208";

    public string Name => "SystemUsingDirectiveOrder";

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
            CheckSystemFirst(group.Regular, ref diagnostics);
        });

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void CheckSystemFirst(List<UsingDirectiveSyntax> regular, ref List<LintDiagnostic>? diagnostics)
    {
        bool seenNonSystem = false;

        foreach (UsingDirectiveSyntax u in regular)
        {
            string name = UsingDirectiveHelper.GetUsingName(u);
            bool isSystem = UsingDirectiveHelper.IsSystemNamespace(name);

            if (isSystem && seenNonSystem)
            {
                FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "System using directives must appear before non-System using directives",
                        Severity = DefaultSeverity,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            if (!isSystem)
            {
                seenNonSystem = true;
            }
        }
    }
}
