using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SwitchExpressionPreferenceRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_prefer_switch_expression";

    public string RuleId => "CSLINT273";

    public string Name => "SwitchExpressionPreference";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool AllSectionsConvertible(SwitchStatementSyntax switchStatement)
    {
        string? assignmentTarget = null;
        bool allReturn = true;
        bool allAssign = true;

        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            if (section.Labels.Count != 1)
            {
                return false;
            }

            StatementSyntax? effectiveStatement = GetEffectiveStatement(section);

            if (effectiveStatement is null)
            {
                return false;
            }

            if (effectiveStatement is ReturnStatementSyntax { Expression: not null })
            {
                allAssign = false;
            }
            else if (effectiveStatement is ExpressionStatementSyntax
                     {
                         Expression: AssignmentExpressionSyntax assignment,
                     }
                     && assignment.Left is IdentifierNameSyntax target)
            {
                allReturn = false;
                string targetName = target.Identifier.Text;

                if (assignmentTarget is null)
                {
                    assignmentTarget = targetName;
                }
                else if (assignmentTarget != targetName)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return allReturn || allAssign;
    }

    private static StatementSyntax? GetEffectiveStatement(SwitchSectionSyntax section)
    {
        if (section.Statements.Count == 1)
        {
            return section.Statements[0] is ReturnStatementSyntax
                ? section.Statements[0]
                : null;
        }

        if (section.Statements.Count == 2 && section.Statements[1] is BreakStatementSyntax)
        {
            return section.Statements[0];
        }

        return null;
    }

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
        if (node is not SwitchStatementSyntax switchStatement)
        {
            return;
        }

        if (switchStatement.Sections.Count < 2)
        {
            return;
        }

        if (!AllSectionsConvertible(switchStatement))
        {
            return;
        }

        FileLinePositionSpan span = switchStatement.SwitchKeyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use switch expression instead of switch statement",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
