using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class LocalOverAnonymousFunctionRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_prefer_local_over_anonymous_function";

    public string RuleId => "IDE0039";

    public string Name => "LocalOverAnonymousFunction";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

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
        // Look for: DelegateType name = lambda/anonymous;
        if (node is not LocalDeclarationStatementSyntax localDecl)
        {
            return;
        }

        if (localDecl.Declaration.Variables.Count != 1)
        {
            return;
        }

        VariableDeclaratorSyntax variable = localDecl.Declaration.Variables[0];

        if (variable.Initializer?.Value is not (LambdaExpressionSyntax
            or AnonymousMethodExpressionSyntax))
        {
            return;
        }

        // Skip var declarations — the type must be explicit (Func<>, Action, etc.)
        // to be a clear candidate for local function conversion
        if (localDecl.Declaration.Type is IdentifierNameSyntax { Identifier.Text: "var" })
        {
            return;
        }

        FileLinePositionSpan span = localDecl.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message =
                    $"Use a local function instead of anonymous function for '{variable.Identifier.Text}'",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
