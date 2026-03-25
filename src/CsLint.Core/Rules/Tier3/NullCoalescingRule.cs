using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0029: Use null coalescing operator (??) instead of ternary null check.
/// </summary>
public sealed class NullCoalescingRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "dotnet_style_coalesce_expression";

    private const string OverrideKey = "dotnet_style_null_checking";

    public string RuleId => "IDE0029";

    public string Name => "NullCoalescing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, OverrideKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax paren)
        {
            expression = paren.Expression;
        }

        return expression;
    }

    private static bool TryGetNullCheckedIdentifier(ExpressionSyntax expression, out string? identifier)
    {
        if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression) &&
                StripParentheses(binary.Left) is IdentifierNameSyntax leftId)
            {
                identifier = leftId.Identifier.Text;
                return true;
            }

            if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression) &&
                StripParentheses(binary.Right) is IdentifierNameSyntax rightId)
            {
                identifier = rightId.Identifier.Text;
                return true;
            }
        }

        identifier = null;
        return false;
    }

    public bool IsEnabled(LintConfiguration configuration)
    {
        if (configuration.GetValue(OverrideKey) is not null)
        {
            return configuration.GetBool(OverrideKey);
        }

        (string? value, string? _) = configuration.GetValueWithSeverity(ConfigKey);

        if (value is not null)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

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
        if (TryGetNullCheckedIdentifier(node.Condition, out string? checkedName) &&
            StripParentheses(node.WhenTrue) is IdentifierNameSyntax trueName &&
            trueName.Identifier.Text == checkedName)
        {
            FileLinePositionSpan span = node.QuestionToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Use null coalescing operator (??) instead of null check with conditional expression",
                    Severity = LintSeverity.Info,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
