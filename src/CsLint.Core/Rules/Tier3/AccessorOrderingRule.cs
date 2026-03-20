using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class AccessorOrderingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_accessor_ordering";

    public string RuleId => "CSLINT263";

    public string Name => "AccessorOrdering";

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

        foreach (BasePropertyDeclarationSyntax property in context.Root.DescendantNodes().OfType<BasePropertyDeclarationSyntax>())
        {
            if (property.AccessorList is not { Accessors.Count: 2 } accessorList)
            {
                continue;
            }

            AccessorDeclarationSyntax first = accessorList.Accessors[0];
            AccessorDeclarationSyntax second = accessorList.Accessors[1];

            // get must come before set/init
            if (first.Kind() is SyntaxKind.SetAccessorDeclaration or SyntaxKind.InitAccessorDeclaration
                && second.Kind() is SyntaxKind.GetAccessorDeclaration)
            {
                string firstKeyword = first.Keyword.Text;
                FileLinePositionSpan span = first.Keyword.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"A get accessor must appear before a {firstKeyword} accessor",
                        Severity = LintSeverity.Warning,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        foreach (EventDeclarationSyntax eventDecl in context.Root.DescendantNodes().OfType<EventDeclarationSyntax>())
        {
            if (eventDecl.AccessorList is not { Accessors.Count: 2 } accessorList)
            {
                continue;
            }

            AccessorDeclarationSyntax first = accessorList.Accessors[0];
            AccessorDeclarationSyntax second = accessorList.Accessors[1];

            // add must come before remove
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
