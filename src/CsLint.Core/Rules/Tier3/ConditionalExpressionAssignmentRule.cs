using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConditionalExpressionAssignmentRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "dotnet_style_prefer_conditional_expression_over_assignment";

    public string RuleId => "CSLINT274";

    public string Name => "ConditionalExpressionAssignment";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static StatementSyntax UnwrapBlock(StatementSyntax statement) =>
        statement is BlockSyntax { Statements.Count: 1 } block ? block.Statements[0] : statement;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitIfStatement(
        IfStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Must have an else clause
        if (node.Else is null)
        {
            return;
        }

        // Both branches must be single assignment to the same variable
        StatementSyntax thenStmt = UnwrapBlock(node.Statement);
        StatementSyntax elseStmt = UnwrapBlock(node.Else.Statement);

        if (thenStmt is not ExpressionStatementSyntax
            {
                Expression: AssignmentExpressionSyntax thenAssign,
            }
            || elseStmt is not ExpressionStatementSyntax
            {
                Expression: AssignmentExpressionSyntax elseAssign,
            })
        {
            return;
        }

        // Must assign to the same target
        if (thenAssign.Left.ToString() != elseAssign.Left.ToString())
        {
            return;
        }

        FileLinePositionSpan span = node.IfKeyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use conditional expression for assignment instead of if-else",
                Severity = LintSeverity.Warning,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
