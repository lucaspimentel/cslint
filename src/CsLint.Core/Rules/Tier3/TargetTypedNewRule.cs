using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class TargetTypedNewRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT212";

    public string Name => "TargetTypedNew";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_implicit_object_creation_when_type_is_apparent"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_implicit_object_creation_when_type_is_apparent") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_implicit_object_creation_when_type_is_apparent");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
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
        if (node is not ObjectCreationExpressionSyntax objectCreation)
        {
            return;
        }

        (string? pref, string? _) = config
            .GetValueWithSeverity("csharp_style_implicit_object_creation_when_type_is_apparent");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Check if parent is a variable declaration with an explicit type that matches
        if (objectCreation.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } })
        {
            if (!declaration.Type.IsVar && TypeNamesMatch(declaration.Type, objectCreation.Type))
            {
                AddDiagnostic(objectCreation, filePath, diagnostics);
                return;
            }
        }

        // Check field declarations: private List<int> _list = new List<int>();
        if (objectCreation.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax fieldDecl } }
            && fieldDecl.Parent is FieldDeclarationSyntax)
        {
            // Already handled by the case above since field declarations also use VariableDeclarationSyntax
            return;
        }
    }

    private static bool TypeNamesMatch(TypeSyntax declaredType, TypeSyntax createdType) =>
        string.Equals(
            declaredType.ToString().TrimEnd('?'),
            createdType.ToString().TrimEnd('?'),
            StringComparison.Ordinal);

    private void AddDiagnostic(
        ObjectCreationExpressionSyntax node,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = node.NewKeyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use target-typed new expression ('new()') when type is apparent",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
