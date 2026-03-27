using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class DotSpacingRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string BeforeKey = "csharp_space_before_dot";

    private const string AfterKey = "csharp_space_after_dot";

    public string RuleId => "CSLINT289";

    public string Name => "DotSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [BeforeKey, AfterKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(BeforeKey) is not null
        || configuration.GetValue(AfterKey) is not null;

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
        if (node is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        SyntaxToken dot = memberAccess.OperatorToken;

        // Space before dot
        string? beforeValue = config.GetValue(BeforeKey);

        if (beforeValue is not null)
        {
            bool requireBefore = string.Equals(beforeValue, "true", StringComparison.OrdinalIgnoreCase);
            bool hasBefore = TriviaHelper.HasLeadingSpace(dot);

            if (requireBefore != hasBefore)
            {
                string action = requireBefore ? "Add" : "Remove";
                FileLinePositionSpan span = dot.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"{action} space before dot",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        // Space after dot
        string? afterValue = config.GetValue(AfterKey);

        if (afterValue is not null)
        {
            bool requireAfter = string.Equals(afterValue, "true", StringComparison.OrdinalIgnoreCase);
            bool hasAfter = TriviaHelper.HasTrailingSpace(dot);

            if (requireAfter != hasAfter)
            {
                string action = requireAfter ? "Add" : "Remove";
                FileLinePositionSpan span = dot.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"{action} space after dot",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
