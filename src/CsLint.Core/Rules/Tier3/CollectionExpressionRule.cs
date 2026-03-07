using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class CollectionExpressionRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT222";

    public string Name => "CollectionExpression";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_prefer_collection_expression"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_prefer_collection_expression") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("dotnet_style_prefer_collection_expression");

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
        switch (node)
        {
            // new int[] { 1, 2, 3 } — explicit array creation with initializer
            case ArrayCreationExpressionSyntax { Initializer: not null } arrayCreation:
                AddDiagnostic(arrayCreation, "Use collection expression instead of explicit array creation", filePath, diagnostics);
                break;

            // new[] { 1, 2, 3 } — implicitly-typed array creation
            case ImplicitArrayCreationExpressionSyntax implicitArray:
                AddDiagnostic(implicitArray, "Use collection expression instead of implicitly-typed array creation", filePath, diagnostics);
                break;

            // Array.Empty<T>() or Enumerable.Empty<T>()
            case InvocationExpressionSyntax invocation:
                CheckEmptyInvocation(invocation, filePath, diagnostics);
                break;
        }
    }

    private void CheckEmptyInvocation(
        InvocationExpressionSyntax invocation,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        // Must have no arguments
        if (invocation.ArgumentList.Arguments.Count != 0)
        {
            return;
        }

        // Match Array.Empty<T>() or Enumerable.Empty<T>()
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name is not GenericNameSyntax { Identifier.Text: "Empty" })
        {
            return;
        }

        if (memberAccess.Expression is not IdentifierNameSyntax identifier)
        {
            return;
        }

        if (identifier.Identifier.Text is not ("Array" or "Enumerable"))
        {
            return;
        }

        AddDiagnostic(invocation, $"Use collection expression instead of '{identifier.Identifier.Text}.Empty<T>()'", filePath, diagnostics);
    }

    private void AddDiagnostic(
        SyntaxNode node,
        string message,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = node.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = message,
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
