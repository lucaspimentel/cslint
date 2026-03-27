using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class NoMultipleWhitespaceRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_no_multiple_whitespace";

    public string RuleId => "SA1025";

    public string Name => "NoMultipleWhitespace";

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
            // Check trailing trivia for whitespace with multiple spaces (not indentation)
            foreach (SyntaxTrivia trivia in token.TrailingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) && trivia.Span.Length > 1)
                {
                    FileLinePositionSpan span = trivia.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "No multiple consecutive whitespace characters",
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
