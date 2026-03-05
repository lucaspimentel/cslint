using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class TypeNamingRule : IRuleDefinition, INamingRuleHandler
{
    public string RuleId => "CSLINT100";

    public string Name => "TypeNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.types_should_be_pascal_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitClassDeclaration(ClassDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "class", diagnostics);

    void INamingRuleHandler.VisitStructDeclaration(StructDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "struct", diagnostics);

    void INamingRuleHandler.VisitEnumDeclaration(EnumDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "enum", diagnostics);

    void INamingRuleHandler.VisitRecordDeclaration(RecordDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "record", diagnostics);

    void INamingRuleHandler.VisitDelegateDeclaration(DelegateDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckName(node.Identifier, "delegate", diagnostics);

    private static void CheckName(SyntaxToken identifier, string kind, List<LintDiagnostic> diagnostics)
    {
        string name = identifier.Text;

        if (!NamingHelper.IsPascalCase(name))
        {
            FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT100",
                    Message = $"{kind} '{name}' should use PascalCase",
                    Severity = LintSeverity.Warning,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
