using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class InterfacePrefixRule : IRuleDefinition, INamingRuleHandler
{
    public string RuleId => "CSLINT101";

    public string Name => "InterfacePrefix";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.interface_should_begin_with_i"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitInterfaceDeclaration(InterfaceDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        string name = node.Identifier.Text;

        if (!NamingHelper.HasPrefix(name, "I") || name.Length < 2 || !char.IsUpper(name[1]))
        {
            FileLinePositionSpan span = node.Identifier.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT101",
                    Message = $"Interface '{name}' should begin with 'I'",
                    Severity = LintSeverity.Warning,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
