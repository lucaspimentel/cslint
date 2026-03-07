using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ExpressionBodiedLocalFunctionsRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT218";

    public string Name => "ExpressionBodiedLocalFunctions";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_expression_bodied_local_functions"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_expression_bodied_local_functions") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_expression_bodied_local_functions");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(pref, "when_on_single_line", StringComparison.OrdinalIgnoreCase))
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
        if (node is not LocalFunctionStatementSyntax localFunc)
        {
            return;
        }

        // Already uses expression body
        if (localFunc.ExpressionBody is not null)
        {
            return;
        }

        if (localFunc.Body is not { Statements.Count: 1 } block)
        {
            return;
        }

        if (block.Statements[0] is not (ReturnStatementSyntax or ExpressionStatementSyntax))
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_expression_bodied_local_functions");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(pref, "when_on_single_line", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        FileLinePositionSpan span = localFunc.Identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Local function can use expression body",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
