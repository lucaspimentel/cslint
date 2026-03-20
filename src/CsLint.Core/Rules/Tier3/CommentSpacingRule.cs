using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class CommentSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_comment_spacing";

    public string RuleId => "CSLINT258";

    public string Name => "CommentSpacing";

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

        foreach (SyntaxTrivia trivia in context.Root.DescendantTrivia())
        {
            if (!trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                continue;
            }

            string text = trivia.ToString();

            // Must start with "//", skip if it's "///" (doc comment) or empty "//"
            if (text.StartsWith("///", StringComparison.Ordinal))
            {
                continue;
            }

            // "//" alone is fine (empty comment)
            if (text.Length == 2)
            {
                continue;
            }

            // Check that the character after "//" is a space
            if (text[2] != ' ')
            {
                FileLinePositionSpan span = trivia.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Single-line comment should begin with a space after '//'",
                        Severity = DefaultSeverity,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        return diagnostics;
    }
}
