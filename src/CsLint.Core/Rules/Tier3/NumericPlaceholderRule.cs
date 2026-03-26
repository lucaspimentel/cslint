using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NumericPlaceholderRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2253.severity";

    public string RuleId => "CA2253";

    public string Name => "NamedPlaceholdersShouldNotBeNumeric";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    // Known logging method names that use message templates
    private static readonly HashSet<string> LoggingMethodNames = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "LogTrace", "LogDebug", "LogInformation", "LogWarning",
        "LogError", "LogCritical", "Log",
    };

    private static bool IsNumericPlaceholder(
        ReadOnlySpan<char> text, int start, out int end)
    {
        end = start;

        // Find opening brace
        if (start >= text.Length || text[start] != '{')
        {
            return false;
        }

        int i = start + 1;

        // Skip whitespace
        while (i < text.Length && char.IsWhiteSpace(text[i]))
        {
            i++;
        }

        // Must have at least one digit
        if (i >= text.Length || !char.IsDigit(text[i]))
        {
            return false;
        }

        // Consume all digits
        while (i < text.Length && char.IsDigit(text[i]))
        {
            i++;
        }

        // Skip optional format specifier after colon or comma
        while (i < text.Length && text[i] != '}')
        {
            i++;
        }

        if (i >= text.Length)
        {
            return false;
        }

        end = i + 1; // past closing brace
        return true;
    }

    private static bool HasNumericPlaceholders(string template)
    {
        ReadOnlySpan<char> text = template.AsSpan();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                // Skip escaped braces {{
                if (i + 1 < text.Length && text[i + 1] == '{')
                {
                    i++;
                    continue;
                }

                if (IsNumericPlaceholder(text, i, out int end))
                {
                    return true;
                }

                i = end > i ? end - 1 : i;
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
            // Get the method name
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

            // Check the first string argument (message template)
            foreach (ArgumentSyntax arg in invocation.ArgumentList.Arguments)
            {
                if (arg.Expression is not LiteralExpressionSyntax literal ||
                    !literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    continue;
                }

                string template = literal.Token.ValueText;

                if (HasNumericPlaceholders(template))
                {
                    FileLinePositionSpan span =
                        literal.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Named placeholders in logging "
                                + "message template should not be numeric",
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
