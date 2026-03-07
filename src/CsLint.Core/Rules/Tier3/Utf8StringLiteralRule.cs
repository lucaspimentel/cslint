using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class Utf8StringLiteralRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT224";

    public string Name => "Utf8StringLiteral";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_utf8_string_literals"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_utf8_string_literals") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_utf8_string_literals");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        // Match Encoding.UTF8.GetBytes("...")
        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: "GetBytes",
                Expression: MemberAccessExpressionSyntax
                {
                    Name.Identifier.Text: "UTF8",
                    Expression: IdentifierNameSyntax { Identifier.Text: "Encoding" },
                },
            })
        {
            return;
        }

        // Must have exactly one argument that is a string literal
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        if (invocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax)
        {
            return;
        }

        FileLinePositionSpan span = invocation.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use UTF-8 string literal ('\"...\"u8') instead of 'Encoding.UTF8.GetBytes(\"...\")'",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
