using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class NoBlankLineAfterOpeningBraceRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_no_blank_line_after_opening_brace";

    public string RuleId => "SA1505";

    public string Name => "NoBlankLineAfterOpeningBrace";

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
                SyntaxToken next = token.GetNextToken();

                if (next != default && !next.IsKind(SyntaxKind.CloseBraceToken) &&
                    NewLineHelper.HasBlankLineBetween(token, next))
                {
                    FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Opening brace should not be followed by a blank line",
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
