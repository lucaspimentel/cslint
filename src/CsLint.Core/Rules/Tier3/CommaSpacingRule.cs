using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class CommaSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_comma_spacing";

    private const string StandardAfterKey = "csharp_space_after_comma";

    private const string StandardBeforeKey = "csharp_space_before_comma";

    public string RuleId => "SA1001";

    public string Name => "CommaSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, StandardAfterKey, StandardBeforeKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasLeadingSpace(SyntaxToken token)
    {
        SyntaxToken previous = token.GetPreviousToken();

        if (previous == default)
        {
            return false;
        }

        foreach (SyntaxTrivia trivia in previous.TrailingTrivia)
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

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);

        if (pref is not null)
        {
            return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Standard keys: space_after_comma=true or space_before_comma=false both enable this rule
        return configuration.GetValue(StandardAfterKey) is not null
            || configuration.GetValue(StandardBeforeKey) is not null;
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
            if (token.IsKind(SyntaxKind.CommaToken))
            {
                FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                // Check no space before comma
                if (HasLeadingSpace(token))
                {
                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "No space should appear before a comma",
                            Severity = DefaultSeverity,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }

                // Check space after comma (unless followed by newline or end of tokens)
                SyntaxToken next = token.GetNextToken();

                if (next != default && !HasTrailingSpaceOrNewline(token))
                {
                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "A comma should be followed by a space",
                            Severity = DefaultSeverity,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }

            token = token.GetNextToken();
        }

        return diagnostics;
    }
}
