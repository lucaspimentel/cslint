using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NewLineBeforeFinallyRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_new_line_before_finally";

    public string RuleId => "CSLINT282";

    public string Name => "NewLineBeforeFinally";

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
        if (node is not FinallyClauseSyntax finallyClause)
        {
            return;
        }

        bool requireNewLine = config.GetBool(ConfigKey);
        bool sameLine = TriviaHelper.IsOnSameLineAsPrevious(finallyClause.FinallyKeyword);

        if (requireNewLine && sameLine && !TriviaHelper.IsOnSameLineAsNext(finallyClause.FinallyKeyword))
        {
            FileLinePositionSpan span = finallyClause.FinallyKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Place 'finally' on a new line",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
        else if (!requireNewLine && !sameLine)
        {
            FileLinePositionSpan span = finallyClause.FinallyKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Place 'finally' on the same line as the closing brace",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
