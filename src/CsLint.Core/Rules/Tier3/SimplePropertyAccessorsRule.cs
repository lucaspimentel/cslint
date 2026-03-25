using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SimplePropertyAccessorsRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "IDE0360";

    public string Name => "SimplePropertyAccessors";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_simple_property_accessors"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static string? TryGetSimpleGetField(AccessorDeclarationSyntax getter)
    {
        // Expression body: get => _field;
        if (getter.ExpressionBody is { Expression: IdentifierNameSyntax exprId })
        {
            return exprId.Identifier.Text;
        }

        // Block body: get { return _field; }
        if (getter.Body is { Statements: [ReturnStatementSyntax { Expression: IdentifierNameSyntax returnId } ] })
        {
            return returnId.Identifier.Text;
        }

        return null;
    }

    private static string? TryGetSimpleSetField(AccessorDeclarationSyntax setter)
    {
        // Expression body: set => _field = value;
        if (setter.ExpressionBody?.Expression is AssignmentExpressionSyntax exprAssign &&
            exprAssign.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
            exprAssign.Left is IdentifierNameSyntax exprLeft &&
            exprAssign.Right is IdentifierNameSyntax exprRight &&
            exprRight.Identifier.Text == "value")
        {
            return exprLeft.Identifier.Text;
        }

        // Block body: set { _field = value; }
        if (setter.Body is
            {
                Statements:
                [
                    ExpressionStatementSyntax
                    {
                        Expression: AssignmentExpressionSyntax blockAssign,
                    },
                ],
            } &&
            blockAssign.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
            blockAssign.Left is IdentifierNameSyntax blockLeft &&
            blockAssign.Right is IdentifierNameSyntax blockRight &&
            blockRight.Identifier.Text == "value")
        {
            return blockLeft.Identifier.Text;
        }

        return null;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_simple_property_accessors") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_simple_property_accessors");

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
        if (node is not PropertyDeclarationSyntax property)
        {
            return;
        }

        if (property.AccessorList is null)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_prefer_simple_property_accessors");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AccessorDeclarationSyntax? getter = null;
        AccessorDeclarationSyntax? setter = null;

        foreach (AccessorDeclarationSyntax accessor in property.AccessorList.Accessors)
        {
            if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
            {
                getter = accessor;
            }
            else if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration) ||
                     accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
            {
                setter = accessor;
            }
        }

        // Need both get and set/init
        if (getter is null || setter is null)
        {
            return;
        }

        // Both must be non-auto (have a body or expression body)
        if (getter.Body is null && getter.ExpressionBody is null)
        {
            return;
        }

        if (setter.Body is null && setter.ExpressionBody is null)
        {
            return;
        }

        string? getField = TryGetSimpleGetField(getter);
        string? setField = TryGetSimpleSetField(setter);

        // Both must be simple forwards to the same field
        if (getField is null || setField is null || getField != setField)
        {
            return;
        }

        FileLinePositionSpan span = property.Identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Property '{property.Identifier.Text}' can be an auto-property (backing field '{getField}')",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
