using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class UsingDeclarationRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT211";

    public string Name => "UsingDeclaration";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_prefer_simple_using_statement"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_prefer_simple_using_statement") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitUsingStatement(
        UsingStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity("csharp_prefer_simple_using_statement");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Skip if this using statement is nested inside another using statement (chained pattern)
        if (node.Parent is UsingStatementSyntax || node.Statement is UsingStatementSyntax)
        {
            return;
        }

        // Only flag using statements with a declaration (not expression-based usings like `using (expr)`)
        if (node.Declaration is null)
        {
            return;
        }

        FileLinePositionSpan span = node.UsingKeyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use simple using statement ('using var') instead of using block",
                Severity = LintSeverity.Warning,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
