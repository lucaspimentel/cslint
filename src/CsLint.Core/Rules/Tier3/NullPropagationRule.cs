using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0031: Use null propagation (?.) instead of ternary null check.
/// Detects patterns like <c>x != null ? x.Property : null</c> that can be simplified to <c>x?.Property</c>.
/// </summary>
public sealed class NullPropagationRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "dotnet_style_null_propagation";

    public string RuleId => "IDE0031";

    public string Name => "NullPropagation";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax paren)
        {
            expression = paren.Expression;
        }

        return expression;
    }

    /// <summary>
    /// Gets the root identifier of a member access chain.
    /// For <c>x.Foo.Bar()</c>, returns "x".
    /// </summary>
    private static string? GetRootIdentifier(ExpressionSyntax expression)
    {
        expression = StripParentheses(expression);

        while (true)
        {
            switch (expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    expression = StripParentheses(memberAccess.Expression);
                    continue;
                case InvocationExpressionSyntax invocation:
                    expression = StripParentheses(invocation.Expression);
                    continue;
                case ElementAccessExpressionSyntax elementAccess:
                    expression = StripParentheses(elementAccess.Expression);
                    continue;
                case IdentifierNameSyntax identifier:
                    return identifier.Identifier.Text;
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Checks whether the expression accesses a member on the given identifier
    /// (e.g., <c>x.Property</c>, <c>x.Method()</c>, <c>x.Foo.Bar</c>).
    /// </summary>
    private static bool IsMemberAccessOnIdentifier(ExpressionSyntax expression, string identifier)
    {
        expression = StripParentheses(expression);

        // Must be a member access, invocation, or element access — not just a bare identifier
        if (expression is IdentifierNameSyntax)
        {
            return false;
        }

        return string.Equals(GetRootIdentifier(expression), identifier, StringComparison.Ordinal);
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitConditionalExpression(
        ConditionalExpressionSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ExpressionSyntax condition = StripParentheses(node.Condition);

        if (condition is not BinaryExpressionSyntax binary)
        {
            return;
        }

        // x != null ? x.Property : null
        if (binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            string? checkedName = GetNullCheckedIdentifier(binary);

            if (checkedName is not null &&
                StripParentheses(node.WhenFalse).IsKind(SyntaxKind.NullLiteralExpression) &&
                IsMemberAccessOnIdentifier(node.WhenTrue, checkedName))
            {
                AddDiagnostic(node.QuestionToken, diagnostics);
            }
        }
        // x == null ? null : x.Property
        else if (binary.IsKind(SyntaxKind.EqualsExpression))
        {
            string? checkedName = GetNullCheckedIdentifier(binary);

            if (checkedName is not null &&
                StripParentheses(node.WhenTrue).IsKind(SyntaxKind.NullLiteralExpression) &&
                IsMemberAccessOnIdentifier(node.WhenFalse, checkedName))
            {
                AddDiagnostic(node.QuestionToken, diagnostics);
            }
        }
    }

    private static string? GetNullCheckedIdentifier(BinaryExpressionSyntax binary)
    {
        if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression) &&
            StripParentheses(binary.Left) is IdentifierNameSyntax leftId)
        {
            return leftId.Identifier.Text;
        }

        if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression) &&
            StripParentheses(binary.Right) is IdentifierNameSyntax rightId)
        {
            return rightId.Identifier.Text;
        }

        return null;
    }

    private void AddDiagnostic(SyntaxToken token, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use null propagation (?.) instead of conditional null check",
                Severity = LintSeverity.Info,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
