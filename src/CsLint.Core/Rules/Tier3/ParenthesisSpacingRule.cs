using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class ParenthesisSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_parenthesis_spacing";

    private const string StandardKey = "csharp_space_between_parentheses";

    public string RuleId => "CSLINT259";

    public string Name => "ParenthesisSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, StandardKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);

        if (pref is not null)
        {
            return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Standard key: "false" means no spaces (matches our no-space check)
        string? standardValue = configuration.GetValue(StandardKey);
        return string.Equals(standardValue, "false", StringComparison.OrdinalIgnoreCase);
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
            if (token.IsKind(SyntaxKind.OpenParenToken))
            {
                // No space after opening paren
                if (TriviaHelper.HasTrailingSpace(token))
                {
                    SyntaxToken next = token.GetNextToken();

                    // Allow space if followed by newline (multi-line expression)
                    if (next != default && !next.IsKind(SyntaxKind.CloseParenToken))
                    {
                        if (!TriviaHelper.HasTrailingNewline(token))
                        {
                            FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                            diagnostics.Add(
                                new LintDiagnostic
                                {
                                    RuleId = RuleId,
                                    Message = "No space should appear after an opening parenthesis",
                                    Severity = DefaultSeverity,
                                    FilePath = span.Path,
                                    Line = span.StartLinePosition.Line + 1,
                                    Column = span.StartLinePosition.Character + 1,
                                });
                        }
                    }
                }
            }
            else if (token.IsKind(SyntaxKind.CloseParenToken))
            {
                // No space before closing paren
                SyntaxToken previous = token.GetPreviousToken();

                if (previous != default && !previous.IsKind(SyntaxKind.OpenParenToken) &&
                    TriviaHelper.HasTrailingSpace(previous) && !TriviaHelper.HasTrailingNewline(previous))
                {
                    FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "No space should appear before a closing parenthesis",
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
