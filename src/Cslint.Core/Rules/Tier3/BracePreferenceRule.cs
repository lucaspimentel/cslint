using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class BracePreferenceRule : IRuleDefinition
{
    public string RuleId => "CSLINT202";

    public string Name => "BracePreference";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_prefer_braces"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_prefer_braces") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity("csharp_prefer_braces");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var walker = new BraceWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class BraceWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            CheckStatement(node.Statement, node.IfKeyword);

            if (node.Else?.Statement is not null and not IfStatementSyntax and not BlockSyntax)
            {
                CheckStatement(node.Else.Statement, node.Else.ElseKeyword);
            }

            base.VisitIfStatement(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            CheckStatement(node.Statement, node.ForKeyword);
            base.VisitForStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            CheckStatement(node.Statement, node.ForEachKeyword);
            base.VisitForEachStatement(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            CheckStatement(node.Statement, node.WhileKeyword);
            base.VisitWhileStatement(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            CheckStatement(node.Statement, node.DoKeyword);
            base.VisitDoStatement(node);
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            // Allow nested using without braces (using cascade)
            if (node.Statement is not UsingStatementSyntax)
            {
                CheckStatement(node.Statement, node.UsingKeyword);
            }

            base.VisitUsingStatement(node);
        }

        private void CheckStatement(StatementSyntax statement, SyntaxToken keyword)
        {
            if (statement is not BlockSyntax)
            {
                FileLinePositionSpan span = keyword.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT202",
                        Message = "Prefer braces for control flow statements",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
