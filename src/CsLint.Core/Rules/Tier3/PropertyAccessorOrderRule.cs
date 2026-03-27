using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1212: Property accessors should follow order — get before set/init.
/// </summary>
public sealed class PropertyAccessorOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_accessor_ordering";

    public string RuleId => "SA1212";

    public string Name => "PropertyAccessorOrder";

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

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
