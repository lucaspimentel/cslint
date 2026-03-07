using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SimpleDefaultExpressionRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT213";

    public string Name => "SimpleDefaultExpression";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_prefer_simple_default_expression"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_prefer_simple_default_expression") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_prefer_simple_default_expression");

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
        // DefaultExpressionSyntax represents `default(T)`
        // LiteralExpressionSyntax with DefaultKeyword represents bare `default`
        if (node is not DefaultExpressionSyntax defaultExpr)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_prefer_simple_default_expression");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        FileLinePositionSpan span = defaultExpr.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Use 'default' instead of 'default({defaultExpr.Type})'",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
