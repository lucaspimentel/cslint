using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NewLineInQueryExpressionRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_new_line_between_query_expression_clauses";

    public string RuleId => "CSLINT285";

    public string Name => "NewLineInQueryExpression";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
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
        if (node is not QueryExpressionSyntax query)
        {
            return;
        }

        bool requireNewLine = config.GetBool(ConfigKey);

        // Check clauses in the query body
        QueryBodySyntax body = query.Body;
        SyntaxNode previousClause = query.FromClause;

        foreach (QueryClauseSyntax clause in body.Clauses)
        {
            CheckClausePair(previousClause, clause, requireNewLine, filePath, diagnostics);
            previousClause = clause;
        }

        // Check the select/group clause
        if (body.SelectOrGroup is not null)
        {
            CheckClausePair(previousClause, body.SelectOrGroup, requireNewLine, filePath, diagnostics);
        }
    }

    private void CheckClausePair(
        SyntaxNode previous,
        SyntaxNode current,
        bool requireNewLine,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        int prevLine = previous.GetLocation().GetLineSpan().EndLinePosition.Line;
        FileLinePositionSpan currSpan = current.GetLocation().GetLineSpan();
        int currLine = currSpan.StartLinePosition.Line;
        bool sameLine = prevLine == currLine;

        if (requireNewLine && sameLine)
        {
            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Place each query expression clause on a new line",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = currSpan.StartLinePosition.Line + 1,
                    Column = currSpan.StartLinePosition.Character + 1,
                });
        }
    }
}
