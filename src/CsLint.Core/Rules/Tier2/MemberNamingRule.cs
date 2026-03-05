using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class MemberNamingRule : IRuleDefinition, INamingRuleHandler
{
    public string RuleId => "CSLINT102";

    public string Name => "MemberNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.members_should_be_pascal_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitMethodDeclaration(MethodDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "method", diagnostics);

    void INamingRuleHandler.VisitPropertyDeclaration(PropertyDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "property", diagnostics);

    void INamingRuleHandler.VisitEventDeclaration(EventDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "event", diagnostics);

    void INamingRuleHandler.VisitEventFieldDeclaration(EventFieldDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
        {
            CheckName(variable.Identifier, "event", diagnostics);
        }
    }

    private static void CheckName(SyntaxToken identifier, string kind, List<LintDiagnostic> diagnostics)
    {
        string name = identifier.Text;

        if (!NamingHelper.IsPascalCase(name))
        {
            FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT102",
                    Message = $"{kind} '{name}' should use PascalCase",
                    Severity = LintSeverity.Warning,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
