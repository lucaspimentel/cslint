using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class BracePreferenceRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT202";

    public string Name => "BracePreference";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_prefer_braces"];

    private bool _active;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_prefer_braces") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity("csharp_prefer_braces");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        _active = true;
        var walker = new CombinedStyleWalker([this]);
        walker.Visit(context.Root);
        _active = false;
        return walker.Diagnostics;
    }

    internal void Initialize(LintConfiguration config)
    {
        (string? pref, string? _) = config.GetValueWithSeverity("csharp_prefer_braces");
        _active = string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    internal void Reset() => _active = false;

    void IStyleRuleHandler.VisitIfStatement(IfStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (!_active)
        {
            return;
        }

        CheckStatement(node.Statement, node.IfKeyword, diagnostics);

        if (node.Else?.Statement is not null and not IfStatementSyntax and not BlockSyntax)
        {
            CheckStatement(node.Else.Statement, node.Else.ElseKeyword, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitForStatement(ForStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckStatement(node.Statement, node.ForKeyword, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitForEachStatement(ForEachStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckStatement(node.Statement, node.ForEachKeyword, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitWhileStatement(WhileStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckStatement(node.Statement, node.WhileKeyword, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitDoStatement(DoStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckStatement(node.Statement, node.DoKeyword, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitUsingStatement(UsingStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active && node.Statement is not UsingStatementSyntax)
        {
            CheckStatement(node.Statement, node.UsingKeyword, diagnostics);
        }
    }

    private static void CheckStatement(StatementSyntax statement, SyntaxToken keyword, List<LintDiagnostic> diagnostics)
    {
        if (statement is not BlockSyntax)
        {
            FileLinePositionSpan span = keyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT202",
                    Message = "Prefer braces for control flow statements",
                    Severity = LintSeverity.Warning,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
