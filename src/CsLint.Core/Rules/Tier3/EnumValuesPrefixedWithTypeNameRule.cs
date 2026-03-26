using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class EnumValuesPrefixedWithTypeNameRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1712.severity";

    public string RuleId => "CA1712";

    public string Name => "DoNotPrefixEnumValuesWithTypeName";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (EnumDeclarationSyntax enumDecl in
            context.Root.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            string typeName = enumDecl.Identifier.Text;

            // Count how many members are prefixed with the type name
            int prefixedCount = 0;

            foreach (EnumMemberDeclarationSyntax member in enumDecl.Members)
            {
                if (member.Identifier.Text.StartsWith(
                    typeName, StringComparison.OrdinalIgnoreCase))
                {
                    prefixedCount++;
                }
            }

            // Only flag if all (or nearly all) members share the prefix
            // matching .NET analyzer behavior: flag if >=75% are prefixed
            if (enumDecl.Members.Count < 2 || prefixedCount == 0)
            {
                continue;
            }

            double ratio = (double)prefixedCount / enumDecl.Members.Count;

            if (ratio < 0.75)
            {
                continue;
            }

            FileLinePositionSpan span =
                enumDecl.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Do not prefix enum values with type name "
                        + $"'{typeName}'",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
