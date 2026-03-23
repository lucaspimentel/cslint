using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class SquareBracketSpacingRule : IRuleDefinition
{
    private const string BeforeOpenKey = "csharp_space_before_open_square_brackets";

    private const string BetweenEmptyKey = "csharp_space_between_empty_square_brackets";

    private const string BetweenKey = "csharp_space_between_square_brackets";

    public string RuleId => "CSLINT290";

    public string Name => "SquareBracketSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [BeforeOpenKey, BetweenEmptyKey, BetweenKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasTrailingSpace(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasLeadingSpace(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(BeforeOpenKey) is not null
        || configuration.GetValue(BetweenEmptyKey) is not null
        || configuration.GetValue(BetweenKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();
        SyntaxToken token = context.Root.GetFirstToken();

        while (token != default)
        {
            if (token.IsKind(SyntaxKind.OpenBracketToken))
            {
                CheckOpenBracket(token, context.Configuration, context.FilePath, diagnostics);
            }

            token = token.GetNextToken();
        }

        return diagnostics;
    }

    private void CheckOpenBracket(
        SyntaxToken openBracket,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        // Space before open bracket
        string? beforeValue = config.GetValue(BeforeOpenKey);

        if (beforeValue is not null)
        {
            bool requireBefore = string.Equals(beforeValue, "true", StringComparison.OrdinalIgnoreCase);
            SyntaxToken prev = openBracket.GetPreviousToken();
            bool hasBefore = prev != default && HasTrailingSpace(prev);

            if (requireBefore != hasBefore)
            {
                AddDiagnostic(openBracket, requireBefore ? "Add" : "Remove", "before open square bracket", filePath, diagnostics);
            }
        }

        // Check if empty brackets
        SyntaxToken next = openBracket.GetNextToken();
        bool isEmpty = next.IsKind(SyntaxKind.CloseBracketToken);

        if (isEmpty)
        {
            string? emptyValue = config.GetValue(BetweenEmptyKey);

            if (emptyValue is not null)
            {
                bool requireSpace = string.Equals(emptyValue, "true", StringComparison.OrdinalIgnoreCase);
                bool hasSpace = HasTrailingSpace(openBracket);

                if (requireSpace != hasSpace)
                {
                    AddDiagnostic(openBracket, requireSpace ? "Add" : "Remove", "between empty square brackets", filePath, diagnostics);
                }
            }
        }
        else
        {
            string? betweenValue = config.GetValue(BetweenKey);

            if (betweenValue is not null)
            {
                bool requireSpace = string.Equals(betweenValue, "true", StringComparison.OrdinalIgnoreCase);
                bool hasSpace = HasTrailingSpace(openBracket);

                if (requireSpace != hasSpace)
                {
                    AddDiagnostic(openBracket, requireSpace ? "Add" : "Remove", "space in square brackets", filePath, diagnostics);
                }
            }
        }
    }

    private void AddDiagnostic(
        SyntaxToken token,
        string action,
        string context,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"{action} {context}",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
