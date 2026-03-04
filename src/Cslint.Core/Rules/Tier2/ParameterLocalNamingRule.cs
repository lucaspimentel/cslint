using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class ParameterLocalNamingRule : IRuleDefinition
{
    public string RuleId => "CSLINT103";

    public string Name => "ParameterLocalNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.locals_should_be_camel_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new ParameterLocalWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class ParameterLocalWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitParameter(ParameterSyntax node)
        {
            // Skip discard parameters
            if (node.Identifier.Text != "_")
            {
                CheckName(node.Identifier, "parameter");
            }

            base.VisitParameter(node);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            // Skip constants (may use PascalCase or UPPER_CASE)
            if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
                {
                    // Skip discards
                    if (variable.Identifier.Text != "_")
                    {
                        CheckName(variable.Identifier, "local variable");
                    }
                }
            }

            base.VisitLocalDeclarationStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            CheckName(node.Identifier, "local variable");
            base.VisitForEachStatement(node);
        }

        private void CheckName(SyntaxToken identifier, string kind)
        {
            string name = identifier.Text;

            if (!NamingHelper.IsCamelCase(name))
            {
                FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT103",
                        Message = $"{kind} '{name}' should use camelCase",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
