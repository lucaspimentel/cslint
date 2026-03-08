using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ArrowExpressionBlankLineRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_allow_blank_line_after_token_in_arrow_expression_clause";

    public string RuleId => "CSLINT233";

    public string Name => "ArrowExpressionBlankLine";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!string.Equals(context.Configuration.GetValue(ConfigKey), "false", StringComparison.OrdinalIgnoreCase))
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
        if (node is not ArrowExpressionClauseSyntax arrow)
        {
            return;
        }

        if (NewLineHelper.HasBlankLineAfter(arrow.ArrowToken))
        {
            FileLinePositionSpan span = arrow.ArrowToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "No blank line allowed after '=>' in arrow expression clause",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
