using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class UsingDirectivePlacementRule : IRuleDefinition
{
    public string RuleId => "CSLINT207";

    public string Name => "UsingDirectivePlacement";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_using_directive_placement"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_using_directive_placement") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity("csharp_using_directive_placement");

        if (pref is null)
        {
            return [];
        }

        bool preferOutside = string.Equals(pref, "outside_namespace", StringComparison.OrdinalIgnoreCase);
        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            if (node is UsingDirectiveSyntax usingDirective)
            {
                bool insideNamespace = usingDirective.Parent is NamespaceDeclarationSyntax;

                if (preferOutside && insideNamespace)
                {
                    FileLinePositionSpan span = usingDirective.UsingKeyword.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Using directives should be placed outside the namespace",
                            Severity = LintSeverity.Warning,
                            FilePath = context.FilePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
                else if (!preferOutside && !insideNamespace && HasNamespace(context.Root))
                {
                    FileLinePositionSpan span = usingDirective.UsingKeyword.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Using directives should be placed inside the namespace",
                            Severity = LintSeverity.Warning,
                            FilePath = context.FilePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }
        }

        return diagnostics;
    }

    private static bool HasNamespace(SyntaxNode root) =>
        root.DescendantNodes().Any(n => n is NamespaceDeclarationSyntax);
}
