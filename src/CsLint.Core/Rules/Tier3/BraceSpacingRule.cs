using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class BraceSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_brace_spacing";

    public string RuleId => "CSLINT260";

    public string Name => "BraceSpacing";

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
            if (token.IsKind(SyntaxKind.OpenBraceToken))
            {
                // Skip string interpolation braces
                if (token.Parent?.IsKind(SyntaxKind.Interpolation) == true)
                {
                    token = token.GetNextToken();
                    continue;
                }

                // Space before opening brace (unless at start of line)
                SyntaxToken previous = token.GetPreviousToken();

                if (previous != default && !TriviaHelper.HasTrailingSpaceOrNewline(previous))
                {
                    FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Opening brace should be preceded by a space",
                            Severity = DefaultSeverity,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }
            else if (token.IsKind(SyntaxKind.CloseBraceToken))
            {
                // Skip string interpolation braces
                if (token.Parent?.IsKind(SyntaxKind.Interpolation) == true)
                {
                    token = token.GetNextToken();
                    continue;
                }

                // Space after closing brace (unless followed by semicolon, comma, paren, or newline)
                SyntaxToken next = token.GetNextToken();

                if (next != default &&
                    !next.IsKind(SyntaxKind.SemicolonToken) &&
                    !next.IsKind(SyntaxKind.CommaToken) &&
                    !next.IsKind(SyntaxKind.CloseParenToken) &&
                    !next.IsKind(SyntaxKind.CloseBraceToken) &&
                    !next.IsKind(SyntaxKind.EndOfFileToken) &&
                    !TriviaHelper.HasTrailingSpaceOrNewline(token))
                {
                    FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Closing brace should be followed by a space",
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
