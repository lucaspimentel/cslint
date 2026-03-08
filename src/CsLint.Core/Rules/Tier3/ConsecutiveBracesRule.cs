using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConsecutiveBracesRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_style_allow_blank_lines_between_consecutive_braces";

    public string RuleId => "CSLINT229";

    public string Name => "ConsecutiveBraces";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!string.Equals(context.Configuration.GetValue(ConfigKey), "false", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();
        SyntaxToken token = context.Root.GetFirstToken();

        while (token != default)
        {
            if (token.IsKind(SyntaxKind.CloseBraceToken))
            {
                SyntaxToken next = token.GetNextToken();

                if (next.IsKind(SyntaxKind.CloseBraceToken) && NewLineHelper.HasBlankLineBetween(token, next))
                {
                    FileLinePositionSpan span = next.GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "No blank line allowed between consecutive closing braces",
                            Severity = LintSeverity.Warning,
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
