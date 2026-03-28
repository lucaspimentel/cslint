using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1013: Closing brace should be followed by a space.
/// </summary>
public sealed class ClosingBraceSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_brace_spacing";

    public string RuleId => "SA1013";

    public string Name => "ClosingBraceSpacing";

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

        List<LintDiagnostic>? diagnostics = null;
        SyntaxToken token = context.Root.GetFirstToken();

        while (token != default)
        {
            if (token.IsKind(SyntaxKind.CloseBraceToken))
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

                    (diagnostics ??= []).Add(
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

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
