using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class IndentationFormattingRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string CaseContentsKey = "csharp_indent_case_contents";

    private const string SwitchLabelsKey = "csharp_indent_switch_labels";

    private const string LabelsKey = "csharp_indent_labels";

    private const string BlockContentsKey = "csharp_indent_block_contents";

    private const string BracesKey = "csharp_indent_braces";

    private const string CaseContentsWhenBlockKey = "csharp_indent_case_contents_when_block";

    private static readonly string[] AllKeys =
        [CaseContentsKey, SwitchLabelsKey, LabelsKey, BlockContentsKey, BracesKey, CaseContentsWhenBlockKey];

    public string RuleId => "CSLINT292";

    public string Name => "IndentationFormatting";

    public IReadOnlyList<string> ConfigKeys { get; } =
        [CaseContentsKey, SwitchLabelsKey, LabelsKey, BlockContentsKey, BracesKey, CaseContentsWhenBlockKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static int GetColumn(SyntaxNodeOrToken nodeOrToken)
    {
        Location location = nodeOrToken.GetLocation()!;
        return location.GetLineSpan().StartLinePosition.Character;
    }

    private static bool IsOnOwnLine(SyntaxToken token)
    {
        SyntaxToken prev = token.GetPreviousToken();

        if (prev == default)
        {
            return true;
        }

        return prev.GetLocation().GetLineSpan().EndLinePosition.Line
            < token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    public bool IsEnabled(LintConfiguration configuration)
    {
        foreach (string key in AllKeys)
        {
            if (configuration.GetValue(key) is not null)
            {
                return true;
            }
        }

        return false;
    }

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
        switch (node)
        {
            case SwitchStatementSyntax switchStmt:
                CheckSwitchLabels(switchStmt, config, filePath, diagnostics);
                CheckCaseContents(switchStmt, config, filePath, diagnostics);
                break;

            case LabeledStatementSyntax labelStmt:
                CheckLabel(labelStmt, config, filePath, diagnostics);
                break;
        }
    }

    private void CheckSwitchLabels(
        SwitchStatementSyntax switchStmt,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (!config.GetBool(SwitchLabelsKey))
        {
            return;
        }

        int switchCol = GetColumn(switchStmt.SwitchKeyword);

        foreach (SwitchSectionSyntax section in switchStmt.Sections)
        {
            SyntaxToken labelToken = section.Labels[0].GetFirstToken();

            if (!IsOnOwnLine(labelToken))
            {
                continue;
            }

            int labelCol = GetColumn(labelToken);

            if (labelCol <= switchCol)
            {
                FileLinePositionSpan span = labelToken.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Indent switch labels",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }

    private void CheckCaseContents(
        SwitchStatementSyntax switchStmt,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (!config.GetBool(CaseContentsKey))
        {
            return;
        }

        foreach (SwitchSectionSyntax section in switchStmt.Sections)
        {
            if (section.Statements.Count == 0)
            {
                continue;
            }

            int labelCol = GetColumn(section.Labels[0].GetFirstToken());
            StatementSyntax firstStmt = section.Statements[0];
            SyntaxToken stmtToken = firstStmt.GetFirstToken();

            if (!IsOnOwnLine(stmtToken))
            {
                continue;
            }

            int stmtCol = GetColumn(stmtToken);

            if (stmtCol <= labelCol)
            {
                FileLinePositionSpan span = stmtToken.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Indent case contents",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }

    private void CheckLabel(
        LabeledStatementSyntax labelStmt,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        string? labelValue = config.GetValue(LabelsKey);

        if (labelValue is null
            || string.Equals(labelValue, "no_change", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SyntaxToken labelToken = labelStmt.Identifier;

        if (!IsOnOwnLine(labelToken))
        {
            return;
        }

        int labelCol = GetColumn(labelToken);

        if (string.Equals(labelValue, "flush_left", StringComparison.OrdinalIgnoreCase)
            && labelCol != 0)
        {
            FileLinePositionSpan span = labelToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Labels should be flush left",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
