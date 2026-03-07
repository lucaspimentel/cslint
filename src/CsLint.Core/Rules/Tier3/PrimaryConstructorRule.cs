using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PrimaryConstructorRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT221";

    public string Name => "PrimaryConstructor";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_primary_constructors"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_primary_constructors") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_primary_constructors");

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
        if (node is not ConstructorDeclarationSyntax constructor)
        {
            return;
        }

        // Skip if constructor has a base() or this() initializer
        if (constructor.Initializer is not null)
        {
            return;
        }

        // Must have a body (not expression-bodied)
        if (constructor.Body is null)
        {
            return;
        }

        // Must have at least one parameter
        if (constructor.ParameterList.Parameters.Count == 0)
        {
            return;
        }

        // Get the parent type
        TypeDeclarationSyntax? typeDecl = constructor.Parent as TypeDeclarationSyntax;

        if (typeDecl is null)
        {
            return;
        }

        // Type must not already have a parameter list (primary constructor)
        if (typeDecl.ParameterList is not null)
        {
            return;
        }

        // Must be the only constructor on the type
        int constructorCount = 0;

        foreach (MemberDeclarationSyntax member in typeDecl.Members)
        {
            if (member is ConstructorDeclarationSyntax)
            {
                constructorCount++;

                if (constructorCount > 1)
                {
                    return;
                }
            }
        }

        // Body must consist only of simple field/property assignments from parameters
        var paramNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (ParameterSyntax param in constructor.ParameterList.Parameters)
        {
            paramNames.Add(param.Identifier.Text);
        }

        foreach (StatementSyntax statement in constructor.Body.Statements)
        {
            if (statement is not ExpressionStatementSyntax
                {
                    Expression: AssignmentExpressionSyntax
                    {
                        RawKind: (int)SyntaxKind.SimpleAssignmentExpression,
                    } assignment,
                })
            {
                return;
            }

            // Right side must be a parameter name
            if (assignment.Right is not IdentifierNameSyntax rightId || !paramNames.Contains(rightId.Identifier.Text))
            {
                return;
            }

            // Left side must be a field/property (identifier, this.x, or _x)
            if (assignment.Left is not (IdentifierNameSyntax or MemberAccessExpressionSyntax
                {
                    Expression: ThisExpressionSyntax,
                }))
            {
                return;
            }
        }

        FileLinePositionSpan span = constructor.Identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Consider using a primary constructor",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
