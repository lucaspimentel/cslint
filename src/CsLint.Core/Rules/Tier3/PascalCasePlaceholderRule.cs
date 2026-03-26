using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PascalCasePlaceholderRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1727.severity";

    public string RuleId => "CA1727";

    public string Name => "UsePascalCaseForNamedPlaceholders";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    // Known logging method names that use message templates
    private static readonly HashSet<string> LoggingMethodNames = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "LogTrace", "LogDebug", "LogInformation", "LogWarning",
        "LogError", "LogCritical", "Log",
    };

    private static bool HasNonPascalCasePlaceholder(string template)
    {
        ReadOnlySpan<char> text = template.AsSpan();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '{')
            {
                continue;
            }

            // Skip escaped braces
            if (i + 1 < text.Length && text[i + 1] == '{')
            {
                i++;
                continue;
            }

            int start = i + 1;

            // Find the placeholder name (before : or })
            int end = start;

            while (end < text.Length &&
                text[end] != '}' &&
                text[end] != ':' &&
                text[end] != ',')
            {
                end++;
            }

            if (end <= start || end >= text.Length)
            {
                continue;
            }

            ReadOnlySpan<char> name = text[start..end].Trim();

            if (name.Length == 0)
            {
                continue;
            }

            // Skip numeric placeholders (CA2253 handles those)
            if (char.IsDigit(name[0]))
            {
                continue;
            }

            // Check if first letter is lowercase → not PascalCase
            if (char.IsLetter(name[0]) && char.IsLower(name[0]))
            {
                return true;
            }

            // Skip to closing brace
            while (i < text.Length && text[i] != '}')
            {
                i++;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (InvocationExpressionSyntax invocation in
            context.Root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>())
        {
            string? methodName = invocation.Expression switch
            {
                MemberAccessExpressionSyntax m => m.Name.Identifier.Text,
                IdentifierNameSyntax id => id.Identifier.Text,
                _ => null,
            };

            if (methodName is null ||
                !LoggingMethodNames.Contains(methodName))
            {
                continue;
            }

            foreach (ArgumentSyntax arg in
                invocation.ArgumentList.Arguments)
            {
                if (arg.Expression is not LiteralExpressionSyntax literal ||
                    !literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    continue;
                }

                string template = literal.Token.ValueText;

                if (HasNonPascalCasePlaceholder(template))
                {
                    FileLinePositionSpan span =
                        literal.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Use PascalCase for named "
                                + "placeholders in logging templates",
                            Severity = DefaultSeverity,
                            FilePath = context.FilePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }

                break; // only check first string arg
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
