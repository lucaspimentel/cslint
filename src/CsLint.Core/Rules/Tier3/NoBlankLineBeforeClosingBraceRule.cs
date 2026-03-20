using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class NoBlankLineBeforeClosingBraceRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_no_blank_line_before_closing_brace";

    public string RuleId => "CSLINT248";

    public string Name => "NoBlankLineBeforeClosingBrace";

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
            if (token.IsKind(SyntaxKind.CloseBraceToken))
            {
                SyntaxToken previous = token.GetPreviousToken();

                if (previous != default && !previous.IsKind(SyntaxKind.OpenBraceToken) &&
                    NewLineHelper.HasBlankLineBetween(previous, token))
                {
                    FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Closing brace should not be preceded by a blank line",
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
