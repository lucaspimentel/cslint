using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

/// <summary>
/// SA1312: Variable names should begin with lower-case letter.
/// </summary>
public sealed class LocalVariableNamingRule : IRuleDefinition, INamingRuleHandler
{
    private const string ConfigKey = "dotnet_naming_rule.locals_should_be_camel_case";

    public string RuleId => "SA1312";

    public string Name => "LocalVariableNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        !NamingConventionRule.HasStandardNamingConfig(configuration);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
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

    private void CheckName(SyntaxToken identifier, string kind, List<LintDiagnostic> diagnostics)
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
                    RuleId = RuleId,
                    Message = $"{kind} '{name}' should use camelCase",
                    Severity = DefaultSeverity,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
