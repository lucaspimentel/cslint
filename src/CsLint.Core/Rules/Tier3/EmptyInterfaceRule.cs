using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class EmptyInterfaceRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1040.severity";

    public string RuleId => "CA1040";

    public string Name => "AvoidEmptyInterfaces";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (InterfaceDeclarationSyntax iface in
            context.Root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
        {
            if (iface.Members.Count > 0)
            {
                continue;
            }

            FileLinePositionSpan span = iface.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Interface '{iface.Identifier.Text}' has no members; "
                        + "consider using a custom attribute instead",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
