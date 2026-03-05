using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class UsingDirectivePlacementRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT207";

    public string Name => "UsingDirectivePlacement";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_using_directive_placement"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_using_directive_placement") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_using_directive_placement");

        if (pref is null)
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (node is not UsingDirectiveSyntax usingDirective)
        {
            return;
        }

        (string? pref, string? _) = config
            .GetValueWithSeverity("csharp_using_directive_placement");

        if (pref is null)
        {
            return;
        }

        bool preferOutside = string.Equals(pref, "outside_namespace", StringComparison.OrdinalIgnoreCase);
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
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
        else if (!preferOutside && !insideNamespace && HasNamespace(node.SyntaxTree.GetRoot()))
        {
            FileLinePositionSpan span = usingDirective.UsingKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Using directives should be placed inside the namespace",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }

    private static bool HasNamespace(SyntaxNode root) =>
        root.DescendantNodes().Any(n => n is NamespaceDeclarationSyntax);
}
