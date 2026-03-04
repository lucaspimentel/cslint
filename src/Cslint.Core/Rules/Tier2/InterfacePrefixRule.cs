using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class InterfacePrefixRule : IRuleDefinition
{
    public string RuleId => "CSLINT101";

    public string Name => "InterfacePrefix";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.interface_should_begin_with_i"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new InterfaceWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class InterfaceWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            string name = node.Identifier.Text;

            if (!NamingHelper.HasPrefix(name, "I") || name.Length < 2 || !char.IsUpper(name[1]))
            {
                FileLinePositionSpan span = node.Identifier.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT101",
                        Message = $"Interface '{name}' should begin with 'I'",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }

            base.VisitInterfaceDeclaration(node);
        }
    }
}
