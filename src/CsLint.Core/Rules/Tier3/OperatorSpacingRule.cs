using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class OperatorSpacingRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_operator_spacing";

    private const string StandardKey = "csharp_space_around_binary_operators";

    public string RuleId => "CSLINT257";

    public string Name => "OperatorSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, StandardKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasLeadingSpace(SyntaxToken token)
    {
        // Check leading trivia of the operator token
        foreach (SyntaxTrivia trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        // Also check trailing trivia of the previous token (Roslyn often puts the space there)
        SyntaxToken previous = token.GetPreviousToken();

        if (previous != default)
        {
            foreach (SyntaxTrivia trivia in previous.TrailingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasTrailingSpace(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);

        if (pref is not null)
        {
            return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Standard key: "before_and_after" enables; "none" or "ignore" disables
        string? standardValue = configuration.GetValue(StandardKey);
        return string.Equals(standardValue, "before_and_after", StringComparison.OrdinalIgnoreCase);
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
        if (node is not BinaryExpressionSyntax binary)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SyntaxToken operatorToken = binary.OperatorToken;

        // Check space before operator
        if (!HasLeadingSpace(operatorToken))
        {
            ReportDiagnostic(operatorToken, "Binary operator should be preceded by a space", filePath, diagnostics);
        }

        // Check space after operator
        if (!HasTrailingSpace(operatorToken))
        {
            ReportDiagnostic(operatorToken, "Binary operator should be followed by a space", filePath, diagnostics);
        }
    }

    private void ReportDiagnostic(SyntaxToken token, string message, string filePath, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = message,
                Severity = DefaultSeverity,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
