using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1012: Opening brace should be preceded by a space.
/// </summary>
public sealed class OpeningBraceSpacingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_brace_spacing";

    public string RuleId => "SA1012";

    public string Name => "OpeningBraceSpacing";

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

                    (diagnostics ??= []).Add(
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

            token = token.GetNextToken();
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
