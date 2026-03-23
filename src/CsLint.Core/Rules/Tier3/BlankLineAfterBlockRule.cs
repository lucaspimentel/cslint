using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class BlankLineAfterBlockRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_allow_blank_line_after_block";

    private const string StandardKey = "dotnet_style_allow_statement_immediately_after_block_experimental";

    public string RuleId => "CSLINT230";

    public string Name => "BlankLineAfterBlock";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, StandardKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsJumpStatement(StatementSyntax statement) =>
        statement is ReturnStatementSyntax
            or ThrowStatementSyntax
            or BreakStatementSyntax
            or ContinueStatementSyntax;

    private static bool EndsWithBlock(StatementSyntax statement)
    {
        SyntaxToken lastToken = statement.GetLastToken();

        // The statement must end with a closing brace
        if (!lastToken.IsKind(SyntaxKind.CloseBraceToken))
        {
            return false;
        }

        // Verify it's from a block (not e.g. an object initializer)
        return lastToken.Parent is BlockSyntax or SwitchStatementSyntax;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetFirstValue(ConfigKey, StandardKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!string.Equals(context.Configuration.GetFirstValue(ConfigKey, StandardKey), "false", StringComparison.OrdinalIgnoreCase))
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
        // We look for statements that contain a block and are followed by another statement
        if (node is not StatementSyntax statement)
        {
            return;
        }

        // Must be inside a block (list of statements)
        if (node.Parent is not BlockSyntax parentBlock)
        {
            return;
        }

        // Statement must end with a closing brace (contains a block)
        if (!EndsWithBlock(statement))
        {
            return;
        }

        // Skip single-line blocks (e.g., `if (x) { p += 2; continue; }`)
        FileLinePositionSpan lineSpan = statement.GetLocation().GetLineSpan();

        if (lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line)
        {
            return;
        }

        // Find the next statement in the parent block
        SyntaxList<StatementSyntax> statements = parentBlock.Statements;
        int index = statements.IndexOf(statement);

        if (index < 0 || index >= statements.Count - 1)
        {
            return;
        }

        StatementSyntax nextStatement = statements[index + 1];

        // Skip guard clause if-chains: consecutive if blocks ending with a jump statement
        if (statement is IfStatementSyntax ifStatement
            && ifStatement.Else is null
            && ifStatement.Statement is BlockSyntax ifBlock
            && ifBlock.Statements.Count > 0
            && IsJumpStatement(ifBlock.Statements.Last()))
        {
            return;
        }

        // Check if there's NO blank line between the closing brace and the next statement
        SyntaxToken closeBrace = statement.GetLastToken();
        SyntaxToken nextFirst = nextStatement.GetFirstToken();

        if (!NewLineHelper.HasBlankLineBetween(closeBrace, nextFirst))
        {
            FileLinePositionSpan span = nextFirst.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Blank line required after a block statement",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
