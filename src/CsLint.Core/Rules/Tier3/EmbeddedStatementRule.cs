using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class EmbeddedStatementRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_allow_embedded_statements_on_same_line";

    public string RuleId => "CSLINT228";

    public string Name => "EmbeddedStatement";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsActive(context.Configuration))
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
        if (!IsActive(config))
        {
            return;
        }

        CheckEmbeddedStatement(node.Statement, node.CloseParenToken, diagnostics);

        if (node.Else?.Statement is not null and not IfStatementSyntax and not BlockSyntax)
        {
            CheckEmbeddedStatement(node.Else.Statement, node.Else.ElseKeyword, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitForStatement(
        ForStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (IsActive(config))
        {
            CheckEmbeddedStatement(node.Statement, node.CloseParenToken, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitForEachStatement(
        ForEachStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (IsActive(config))
        {
            CheckEmbeddedStatement(node.Statement, node.CloseParenToken, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitWhileStatement(
        WhileStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (IsActive(config))
        {
            CheckEmbeddedStatement(node.Statement, node.CloseParenToken, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitDoStatement(
        DoStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (IsActive(config))
        {
            CheckEmbeddedStatement(node.Statement, node.DoKeyword, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitUsingStatement(
        UsingStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (IsActive(config))
        {
            CheckEmbeddedStatement(node.Statement, node.CloseParenToken, diagnostics);
        }
    }

    private static bool IsActive(LintConfiguration config) =>
        string.Equals(config.GetValue(ConfigKey), "false", StringComparison.OrdinalIgnoreCase);

    private void CheckEmbeddedStatement(
        StatementSyntax statement,
        SyntaxToken precedingToken,
        List<LintDiagnostic> diagnostics)
    {
        if (statement is BlockSyntax)
        {
            return;
        }

        FileLinePositionSpan precedingSpan = precedingToken.GetLocation().GetLineSpan();
        FileLinePositionSpan statementSpan = statement.GetLocation().GetLineSpan();

        if (precedingSpan.StartLinePosition.Line == statementSpan.StartLinePosition.Line)
        {
            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Embedded statement must be on its own line",
                    Severity = LintSeverity.Warning,
                    FilePath = statementSpan.Path,
                    Line = statementSpan.StartLinePosition.Line + 1,
                    Column = statementSpan.StartLinePosition.Character + 1,
                });
        }
    }
}
