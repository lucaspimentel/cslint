using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class FieldNamingRule : IRuleDefinition
{
    public string RuleId => "CSLINT104";

    public string Name => "FieldNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.private_fields_should_be_underscore_camel_case"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new FieldWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class FieldWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            // Only check private/internal instance fields (not const, not static, not events)
            if (node.Modifiers.Any(SyntaxKind.ConstKeyword) ||
                node.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return;
            }

            bool isPrivate = !node.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                             !node.Modifiers.Any(SyntaxKind.ProtectedKeyword);

            if (!isPrivate)
            {
                return;
            }

            foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
            {
                string name = variable.Identifier.Text;

                if (!NamingHelper.IsUnderscoreCamelCase(name))
                {
                    FileLinePositionSpan span = variable.Identifier.GetLocation().GetLineSpan();

                    Diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = "CSLINT104",
                            Message = $"Private field '{name}' should use _camelCase",
                            Severity = LintSeverity.Warning,
                            FilePath = filePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }
        }
    }
}
