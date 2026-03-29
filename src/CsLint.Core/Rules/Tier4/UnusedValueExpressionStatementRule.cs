using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class UnusedValueExpressionStatementRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "IDE0058";

    public string Name => "UnusedValueExpressionStatement";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.IDE0058.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.IDE0058.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        foreach (ExpressionStatementSyntax statement in context.Root.DescendantNodes().OfType<ExpressionStatementSyntax>())
        {
            ExpressionSyntax expression = statement.Expression;

            // Skip assignments — the value is being captured
            if (expression is AssignmentExpressionSyntax)
            {
                continue;
            }

            // Skip increment/decrement — side-effect is the point
            if (expression is PostfixUnaryExpressionSyntax postfix &&
                (postfix.IsKind(SyntaxKind.PostIncrementExpression) || postfix.IsKind(SyntaxKind.PostDecrementExpression)))
            {
                continue;
            }

            if (expression is PrefixUnaryExpressionSyntax prefix &&
                (prefix.IsKind(SyntaxKind.PreIncrementExpression) || prefix.IsKind(SyntaxKind.PreDecrementExpression)))
            {
                continue;
            }

            // Skip throw expressions
            if (expression is ThrowExpressionSyntax)
            {
                continue;
            }

            TypeInfo typeInfo = model.GetTypeInfo(expression);
            ITypeSymbol? type = typeInfo.Type;

            // No type info or error type — skip
            if (type is null or IErrorTypeSymbol)
            {
                continue;
            }

            // Void return — nothing to capture
            if (type.SpecialType == SpecialType.System_Void)
            {
                continue;
            }

            LinePosition start = expression.GetLocation().GetLineSpan().StartLinePosition;

            diagnostics.Add(new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Expression value of type '{type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' is unused",
                Severity = DefaultSeverity,
                FilePath = context.FilePath,
                Line = start.Line + 1,
                Column = start.Character + 1,
            });
        }
    }
}
