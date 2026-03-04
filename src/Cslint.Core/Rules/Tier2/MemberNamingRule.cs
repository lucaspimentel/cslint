using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class MemberNamingRule : IRuleDefinition
{
    public string RuleId => "CSLINT102";

    public string Name => "MemberNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.members_should_be_pascal_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new MemberWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class MemberWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node) =>
            CheckName(node.Identifier, "method");

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node) =>
            CheckName(node.Identifier, "property");

        public override void VisitEventDeclaration(EventDeclarationSyntax node) =>
            CheckName(node.Identifier, "event");

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
            {
                CheckName(variable.Identifier, "event");
            }
        }

        private void CheckName(SyntaxToken identifier, string kind)
        {
            string name = identifier.Text;

            if (!NamingHelper.IsPascalCase(name))
            {
                FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT102",
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
