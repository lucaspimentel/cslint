using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PreserveSingleLineRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string StatementsKey = "csharp_preserve_single_line_statements";

    private const string BlocksKey = "csharp_preserve_single_line_blocks";

    public string RuleId => "CSLINT293";

    public string Name => "PreserveSingleLine";

    public IReadOnlyList<string> ConfigKeys { get; } = [StatementsKey, BlocksKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsSingleLine(SyntaxNode node)
    {
        FileLinePositionSpan span = node.GetLocation().GetLineSpan();
        return span.StartLinePosition.Line == span.EndLinePosition.Line;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(StatementsKey) is not null
        || configuration.GetValue(BlocksKey) is not null;

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
        // Single-line statements: if (x) return; (no braces, on one line)
        string? stmtValue = config.GetValue(StatementsKey);

        if (string.Equals(stmtValue, "false", StringComparison.OrdinalIgnoreCase)
            && node is IfStatementSyntax { Statement: not BlockSyntax } ifStmt
            && IsSingleLine(ifStmt))
        {
            FileLinePositionSpan span = ifStmt.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Do not place statement on same line as control flow keyword",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        // Single-line blocks: { return x; } on one line
        string? blockValue = config.GetValue(BlocksKey);

        if (string.Equals(blockValue, "false", StringComparison.OrdinalIgnoreCase)
            && node is BlockSyntax { Statements.Count: > 0 } block
            && IsSingleLine(block))
        {
            FileLinePositionSpan span = block.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Do not place block on a single line",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
