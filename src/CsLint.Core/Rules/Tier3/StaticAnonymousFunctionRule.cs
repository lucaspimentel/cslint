using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class StaticAnonymousFunctionRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "IDE0320";

    public string Name => "StaticAnonymousFunction";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_prefer_static_anonymous_function"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_prefer_static_anonymous_function") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_prefer_static_anonymous_function");

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
        if (node is not AnonymousFunctionExpressionSyntax anonFunc)
        {
            return;
        }

        // Already static
        if (anonFunc.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_prefer_static_anonymous_function");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (CaptureAnalysis.CapturesEnclosingState(anonFunc))
        {
            return;
        }

        SyntaxToken keyword = anonFunc switch
        {
            AnonymousMethodExpressionSyntax a => a.DelegateKeyword,
            _ => anonFunc.GetFirstToken(),
        };

        FileLinePositionSpan span = keyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Anonymous function can be made static",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
