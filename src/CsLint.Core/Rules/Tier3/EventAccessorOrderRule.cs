using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1213: Event accessors should follow order — add before remove.
/// </summary>
public sealed class EventAccessorOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_accessor_ordering";

    public string RuleId => "SA1213";

    public string Name => "EventAccessorOrder";

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

        foreach (EventDeclarationSyntax eventDecl in context.Root.DescendantNodes().OfType<EventDeclarationSyntax>())
        {
            if (eventDecl.AccessorList is not { Accessors.Count: 2 } accessorList)
            {
                continue;
            }

            AccessorDeclarationSyntax first = accessorList.Accessors[0];
            AccessorDeclarationSyntax second = accessorList.Accessors[1];

            if (first.Kind() is SyntaxKind.RemoveAccessorDeclaration
                && second.Kind() is SyntaxKind.AddAccessorDeclaration)
            {
                FileLinePositionSpan span = first.Keyword.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "An add accessor must appear before a remove accessor",
                        Severity = LintSeverity.Warning,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
