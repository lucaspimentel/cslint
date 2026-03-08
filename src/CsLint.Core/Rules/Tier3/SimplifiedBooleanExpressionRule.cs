using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SimplifiedBooleanExpressionRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT235";

    public string Name => "SimplifiedBooleanExpression";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_prefer_simplified_boolean_expressions"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_prefer_simplified_boolean_expressions") is not null;

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
        (string? pref, string? _) = config.GetValueWithSeverity("dotnet_style_prefer_simplified_boolean_expressions");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        bool whenTrueIsBool = IsBooleanLiteral(node.WhenTrue);
        bool whenFalseIsBool = IsBooleanLiteral(node.WhenFalse);

        if (!whenTrueIsBool && !whenFalseIsBool)
        {
            return;
        }

        FileLinePositionSpan span = node.QuestionToken.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Conditional expression can be simplified",
                Severity = LintSeverity.Info,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private static bool IsBooleanLiteral(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal &&
        (literal.IsKind(SyntaxKind.TrueLiteralExpression) || literal.IsKind(SyntaxKind.FalseLiteralExpression));
}
