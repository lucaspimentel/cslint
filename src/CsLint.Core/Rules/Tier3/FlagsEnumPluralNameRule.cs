using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class FlagsEnumPluralNameRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1714.severity";

    public string RuleId => "CA1714";

    public string Name => "FlagsEnumsShouldHavePluralNames";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasFlagsAttribute(EnumDeclarationSyntax enumDecl)
    {
        foreach (AttributeListSyntax attrList in enumDecl.AttributeLists)
        {
            foreach (AttributeSyntax attr in attrList.Attributes)
            {
                string name = attr.Name.ToString();

                if (name is "Flags" or "FlagsAttribute" or
                    "System.Flags" or "System.FlagsAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsPluralName(string name)
    {
        if (name.Length < 2)
        {
            return false;
        }

        // Simple heuristic: ends with 's' but not 'ss' (e.g., "Status")
        return name.EndsWith('s') && !name.EndsWith("ss", StringComparison.Ordinal);
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

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
            if (!HasFlagsAttribute(enumDecl))
            {
                continue;
            }

            if (IsPluralName(enumDecl.Identifier.Text))
            {
                continue;
            }

            FileLinePositionSpan span =
                enumDecl.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"[Flags] enum '{enumDecl.Identifier.Text}' "
                        + "should have a plural name",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
