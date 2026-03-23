using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConditionalExpressionReturnRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "dotnet_style_prefer_conditional_expression_over_return";

    public string RuleId => "CSLINT275";

    public string Name => "ConditionalExpressionReturn";

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

        // Both branches must be single return statements with expressions
        StatementSyntax thenStmt = UnwrapBlock(node.Statement);
        StatementSyntax elseStmt = UnwrapBlock(node.Else.Statement);

        if (thenStmt is not ReturnStatementSyntax { Expression: not null }
            || elseStmt is not ReturnStatementSyntax { Expression: not null })
        {
            return;
        }

        FileLinePositionSpan span = node.IfKeyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use conditional expression for return instead of if-else",
                Severity = LintSeverity.Warning,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
