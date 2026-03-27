using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SingleStatementPerLineRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_single_statement_per_line";

    public string RuleId => "SA1107";

    public string Name => "SingleStatementPerLine";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
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
        if (node is not BlockSyntax block)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        for (int i = 1; i < block.Statements.Count; i++)
        {
            StatementSyntax previous = block.Statements[i - 1];
            StatementSyntax current = block.Statements[i];

            int previousEndLine = previous.GetLocation().GetLineSpan().EndLinePosition.Line;
            FileLinePositionSpan currentSpan = current.GetLocation().GetLineSpan();
            int currentStartLine = currentSpan.StartLinePosition.Line;

            if (currentStartLine == previousEndLine)
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Each statement should be on its own line",
                        Severity = DefaultSeverity,
                        FilePath = filePath,
                        Line = currentSpan.StartLinePosition.Line + 1,
                        Column = currentSpan.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
