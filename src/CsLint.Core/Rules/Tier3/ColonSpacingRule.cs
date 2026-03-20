using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class ColonSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_colon_spacing";

    public string RuleId => "CSLINT261";

    public string Name => "ColonSpacing";

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
            if (token.IsKind(SyntaxKind.ColonToken))
            {
                // Check for base list colons, conditional expression colons, case label colons
                // These should have space before and after
                SyntaxNode? parent = token.Parent;

                if (parent is not null && IsBaseListOrConditionalColon(parent, token))
                {
                    SyntaxToken previous = token.GetPreviousToken();

                    if (previous != default && !HasTrailingSpace(previous))
                    {
                        ReportDiagnostic(token, "Colon should be preceded by a space", diagnostics);
                    }

                    if (!HasTrailingSpaceOrNewline(token))
                    {
                        ReportDiagnostic(token, "Colon should be followed by a space", diagnostics);
                    }
                }
            }

            token = token.GetNextToken();
        }

        return diagnostics;
    }

    private static bool IsBaseListOrConditionalColon(SyntaxNode parent, SyntaxToken colon) =>
        parent.IsKind(SyntaxKind.BaseList) ||
        parent.IsKind(SyntaxKind.SimpleBaseType) ||
        parent.IsKind(SyntaxKind.TypeParameterConstraintClause) ||
        parent.IsKind(SyntaxKind.BaseConstructorInitializer) ||
        parent.IsKind(SyntaxKind.ThisConstructorInitializer);

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

    private static bool HasTrailingSpaceOrNewline(SyntaxToken token)
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
