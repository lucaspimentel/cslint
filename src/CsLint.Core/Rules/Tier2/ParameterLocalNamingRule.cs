using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class ParameterLocalNamingRule : IRuleDefinition, INamingRuleHandler
{
    public string RuleId => "CSLINT103";

    public string Name => "ParameterLocalNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.locals_should_be_camel_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitParameter(ParameterSyntax node, List<LintDiagnostic> diagnostics)
    {
        // Skip discard parameters
        if (node.Identifier.ValueText == "_")
        {
            return;
        }

        // Skip record positional parameters (they generate public properties, PascalCase is correct)
        if (node.Parent?.Parent is RecordDeclarationSyntax)
        {
            return;
        }

        CheckName(node.Identifier, "parameter", diagnostics);
    }

    void INamingRuleHandler.VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        // Skip constants (may use PascalCase or UPPER_CASE)
        if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
            {
                // Skip discards
                if (variable.Identifier.ValueText != "_")
                {
                    CheckName(variable.Identifier, "local variable", diagnostics);
                }
            }
        }
    }

    void INamingRuleHandler.VisitForEachStatement(ForEachStatementSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "local variable", diagnostics);

    private static void CheckName(SyntaxToken identifier, string kind, List<LintDiagnostic> diagnostics)
    {
        string name = identifier.ValueText;

        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (!NamingHelper.IsCamelCase(name))
        {
            FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT103",
                    Message = $"{kind} '{name}' should use camelCase",
                    Severity = LintSeverity.Warning,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
