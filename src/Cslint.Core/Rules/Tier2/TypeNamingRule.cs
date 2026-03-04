using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class TypeNamingRule : IRuleDefinition
{
    public string RuleId => "CSLINT100";

    public string Name => "TypeNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.types_should_be_pascal_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new TypeNamingWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class TypeNamingWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitClassDeclaration(ClassDeclarationSyntax node) =>
            CheckName(node.Identifier, "class");

        public override void VisitStructDeclaration(StructDeclarationSyntax node) =>
            CheckName(node.Identifier, "struct");

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node) =>
            CheckName(node.Identifier, "enum");

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node) =>
            CheckName(node.Identifier, "record");

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node) =>
            CheckName(node.Identifier, "delegate");

        private void CheckName(SyntaxToken identifier, string kind)
        {
            string name = identifier.Text;

            if (!NamingHelper.IsPascalCase(name))
            {
                FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT100",
                        Message = $"{kind} '{name}' should use PascalCase",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
