using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class IndexOperatorRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT226";

    public string Name => "IndexOperator";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_index_operator"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_index_operator") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_index_operator");

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
        // Match arr[arr.Length - 1] or arr[arr.Count - 1]
        if (node is not ElementAccessExpressionSyntax elementAccess)
        {
            return;
        }

        if (elementAccess.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        ExpressionSyntax indexExpr = elementAccess.ArgumentList.Arguments[0].Expression;

        // Must be a subtraction: something - N
        if (indexExpr is not BinaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.SubtractExpression,
                Left: MemberAccessExpressionSyntax lengthAccess,
                Right: LiteralExpressionSyntax,
            })
        {
            return;
        }

        // The property must be .Length or .Count
        string propertyName = lengthAccess.Name.Identifier.Text;

        if (propertyName is not ("Length" or "Count"))
        {
            return;
        }

        // The receiver of .Length must match the receiver of the element access
        string receiver = elementAccess.Expression.ToString();
        string lengthReceiver = lengthAccess.Expression.ToString();

        if (!string.Equals(receiver, lengthReceiver, StringComparison.Ordinal))
        {
            return;
        }

        FileLinePositionSpan span = elementAccess.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Use index operator ('^') instead of '{propertyName} - n'",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
