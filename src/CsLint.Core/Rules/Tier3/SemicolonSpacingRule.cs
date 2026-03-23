using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class SemicolonSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_semicolon_spacing";

    private const string StandardBeforeKey = "csharp_space_before_semicolon_in_for_statement";

    private const string StandardAfterKey = "csharp_space_after_semicolon_in_for_statement";

    public string RuleId => "CSLINT256";

    public string Name => "SemicolonSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, StandardBeforeKey, StandardAfterKey];

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

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);

        if (pref is not null)
        {
            return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
        }

        return configuration.GetValue(StandardBeforeKey) is not null
            || configuration.GetValue(StandardAfterKey) is not null;
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
            if (token.IsKind(SyntaxKind.SemicolonToken))
            {
                // Check no space before semicolon
                SyntaxToken previous = token.GetPreviousToken();

                if (previous != default && HasTrailingSpace(previous))
                {
                    FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "No space should appear before a semicolon",
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
