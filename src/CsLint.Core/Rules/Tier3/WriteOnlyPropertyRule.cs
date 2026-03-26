using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class WriteOnlyPropertyRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1044.severity";

    public string RuleId => "CA1044";

    public string Name => "PropertiesShouldNotBeWriteOnly";

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

        foreach (PropertyDeclarationSyntax property in
            context.Root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (property.AccessorList is null)
            {
                continue;
            }

            bool hasSetter = false;
            bool hasGetter = false;

            foreach (AccessorDeclarationSyntax accessor in
                property.AccessorList.Accessors)
            {
                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                {
                    hasGetter = true;
                }
                else if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration) ||
                    accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
                {
                    hasSetter = true;
                }
            }

            if (hasSetter && !hasGetter)
            {
                FileLinePositionSpan span =
                    property.Identifier.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Property '{property.Identifier.Text}' is "
                            + "write-only; add a getter",
                        Severity = DefaultSeverity,
                        FilePath = context.FilePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
