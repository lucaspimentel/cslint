using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NamespaceDeclarationRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT203";

    public string Name => "NamespaceDeclaration";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_namespace_declarations"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_namespace_declarations") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_namespace_declarations");

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
        (string? pref, string? _) = config
            .GetValueWithSeverity("csharp_style_namespace_declarations");

        if (pref is null)
        {
            return;
        }

        bool preferFileScoped = string.Equals(pref, "file_scoped", StringComparison.OrdinalIgnoreCase);

        if (preferFileScoped && node is NamespaceDeclarationSyntax ns)
        {
            FileLinePositionSpan span = ns.NamespaceKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Use file-scoped namespace declaration",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
        else if (!preferFileScoped && node is FileScopedNamespaceDeclarationSyntax fileScopedNs)
        {
            FileLinePositionSpan span = fileScopedNs.NamespaceKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Use block-scoped namespace declaration",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
