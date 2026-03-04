using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class ConstantNamingRule : IRuleDefinition
{
    public string RuleId => "CSLINT105";

    public string Name => "ConstantNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.constants_should_be_pascal_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new ConstantWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class ConstantWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                return;
            }

            foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
            {
                string name = variable.Identifier.Text;

                // Accept PascalCase or UPPER_CASE
                if (!NamingHelper.IsPascalCase(name) && !NamingHelper.IsUpperCase(name))
                {
                    FileLinePositionSpan span = variable.Identifier.GetLocation().GetLineSpan();

                    Diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = "CSLINT105",
                            Message = $"Constant '{name}' should use PascalCase or UPPER_CASE",
                            Severity = LintSeverity.Warning,
                            FilePath = filePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                return;
            }

            foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
            {
                string name = variable.Identifier.Text;

                if (!NamingHelper.IsPascalCase(name) && !NamingHelper.IsUpperCase(name))
                {
                    FileLinePositionSpan span = variable.Identifier.GetLocation().GetLineSpan();

                    Diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = "CSLINT105",
                            Message = $"Constant '{name}' should use PascalCase or UPPER_CASE",
                            Severity = LintSeverity.Warning,
                            FilePath = filePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }

            base.VisitLocalDeclarationStatement(node);
        }
    }
}
