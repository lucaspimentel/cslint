using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class KeywordSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_keyword_spacing";

    private static readonly HashSet<SyntaxKind> SpaceAfterKeywords =
    [
        SyntaxKind.IfKeyword,
        SyntaxKind.ElseKeyword,
        SyntaxKind.ForKeyword,
        SyntaxKind.ForEachKeyword,
        SyntaxKind.WhileKeyword,
        SyntaxKind.DoKeyword,
        SyntaxKind.SwitchKeyword,
        SyntaxKind.CatchKeyword,
        SyntaxKind.UsingKeyword,
        SyntaxKind.LockKeyword,
        SyntaxKind.ReturnKeyword,
        SyntaxKind.ThrowKeyword,
        SyntaxKind.YieldKeyword,
        SyntaxKind.AwaitKeyword,
        SyntaxKind.CaseKeyword,
        SyntaxKind.WhenKeyword,
    ];

    private static readonly HashSet<SyntaxKind> NoSpaceAfterKeywords =
    [
        SyntaxKind.TypeOfKeyword,
        SyntaxKind.SizeOfKeyword,
        SyntaxKind.DefaultKeyword,
    ];

    public string RuleId => "CSLINT254";

    public string Name => "KeywordSpacing";

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
        SyntaxToken token = context.Root.GetFirstToken();

        while (token != default)
        {
            SyntaxKind kind = token.Kind();

            if (SpaceAfterKeywords.Contains(kind))
            {
                SyntaxToken next = token.GetNextToken();

                if (next != default && !HasTrailingSpace(token) && !next.IsKind(SyntaxKind.SemicolonToken))
                {
                    ReportDiagnostic(token, $"Keyword '{token.Text}' should be followed by a space", diagnostics);
                }
            }
            else if (NoSpaceAfterKeywords.Contains(kind))
            {
                SyntaxToken next = token.GetNextToken();

                if (next != default && next.IsKind(SyntaxKind.OpenParenToken) && HasTrailingSpace(token))
                {
                    ReportDiagnostic(token, $"Keyword '{token.Text}' should not be followed by a space", diagnostics);
                }
            }

            token = token.GetNextToken();
        }

        return diagnostics;
    }

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

    private void ReportDiagnostic(SyntaxToken token, string message, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = message,
                Severity = DefaultSeverity,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
