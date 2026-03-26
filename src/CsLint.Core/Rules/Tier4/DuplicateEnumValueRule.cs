using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class DuplicateEnumValueRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "CA1069";

    public string Name => "DuplicateEnumValue";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.CA1069.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.CA1069.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        foreach (EnumDeclarationSyntax enumDecl in context.Root.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            var seen = new Dictionary<object, EnumMemberDeclarationSyntax>();

            foreach (EnumMemberDeclarationSyntax member in enumDecl.Members)
            {
                if (model.GetDeclaredSymbol(member) is not IFieldSymbol { ConstantValue: not null } field)
                {
                    continue;
                }

                object value = field.ConstantValue;

                if (seen.TryGetValue(value, out EnumMemberDeclarationSyntax? first))
                {
                    LinePosition start = member.Identifier.GetLocation().GetLineSpan().StartLinePosition;

                    diagnostics.Add(new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Enum member '{member.Identifier.Text}' has the same value ({value}) as '{first.Identifier.Text}'",
                        Severity = DefaultSeverity,
                        FilePath = context.FilePath,
                        Line = start.Line + 1,
                        Column = start.Character + 1,
                    });
                }
                else
                {
                    seen[value] = member;
                }
            }
        }
    }
}
