using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ObjectInitializerRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT215";

    public string Name => "ObjectInitializer";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_object_initializer"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_object_initializer") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("dotnet_style_object_initializer");

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
        if (node is not LocalDeclarationStatementSyntax localDecl)
        {
            return;
        }

        if (localDecl.Declaration.Variables.Count != 1)
        {
            return;
        }

        VariableDeclaratorSyntax variable = localDecl.Declaration.Variables[0];

        if (variable.Initializer?.Value is not ObjectCreationExpressionSyntax creation)
        {
            return;
        }

        // Already has an initializer
        if (creation.Initializer is not null)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("dotnet_style_object_initializer");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string varName = variable.Identifier.Text;

        // Check if the local declaration is inside a block
        if (localDecl.Parent is not BlockSyntax block)
        {
            return;
        }

        int declIndex = block.Statements.IndexOf(localDecl);

        if (declIndex < 0 || declIndex + 1 >= block.Statements.Count)
        {
            return;
        }

        // Look for consecutive `varName.Property = value;` statements
        bool hasPropertyAssignment = false;

        for (int i = declIndex + 1; i < block.Statements.Count; i++)
        {
            if (block.Statements[i] is not ExpressionStatementSyntax
                {
                    Expression: AssignmentExpressionSyntax
                    {
                        RawKind: (int)SyntaxKind.SimpleAssignmentExpression,
                        Left: MemberAccessExpressionSyntax memberAccess,
                    }
                })
            {
                break;
            }

            if (memberAccess.Expression is not IdentifierNameSyntax identifier ||
                identifier.Identifier.Text != varName)
            {
                break;
            }

            hasPropertyAssignment = true;
        }

        if (!hasPropertyAssignment)
        {
            return;
        }

        FileLinePositionSpan span = localDecl.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use object initializer",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
