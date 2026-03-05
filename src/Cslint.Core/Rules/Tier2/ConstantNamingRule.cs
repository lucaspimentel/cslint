using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class ConstantNamingRule : IRuleDefinition, INamingRuleHandler
{
    public string RuleId => "CSLINT105";

    public string Name => "ConstantNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.constants_should_be_pascal_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
        {
            CheckConstant(variable.Identifier, diagnostics);
        }
    }

    void INamingRuleHandler.VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
        {
            CheckConstant(variable.Identifier, diagnostics);
        }
    }

    private static void CheckConstant(SyntaxToken identifier, List<LintDiagnostic> diagnostics)
    {
        string name = identifier.Text;

        // Accept PascalCase or UPPER_CASE
        if (!NamingHelper.IsPascalCase(name) && !NamingHelper.IsUpperCase(name))
        {
            FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT105",
                    Message = $"Constant '{name}' should use PascalCase or UPPER_CASE",
                    Severity = LintSeverity.Warning,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
