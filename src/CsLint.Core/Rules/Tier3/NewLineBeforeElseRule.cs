using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NewLineBeforeElseRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_new_line_before_else";

    public string RuleId => "CSLINT280";

    public string Name => "NewLineBeforeElse";

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

        bool requireNewLine = context.Configuration.GetBool(ConfigKey);
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
        if (node is not ElseClauseSyntax elseClause)
        {
            return;
        }

        bool requireNewLine = config.GetBool(ConfigKey);
        bool sameLine = TriviaHelper.IsOnSameLineAsPrevious(elseClause.ElseKeyword);

        if (requireNewLine && sameLine && !TriviaHelper.IsOnSameLineAsNext(elseClause.ElseKeyword))
        {
            FileLinePositionSpan span = elseClause.ElseKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Place 'else' on a new line",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
        else if (!requireNewLine && !sameLine)
        {
            FileLinePositionSpan span = elseClause.ElseKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Place 'else' on the same line as the closing brace",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
