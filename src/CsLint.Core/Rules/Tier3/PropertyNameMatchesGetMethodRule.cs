using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PropertyNameMatchesGetMethodRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1721.severity";

    public string RuleId => "CA1721";

    public string Name => "PropertyNamesShouldNotMatchGetMethods";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsVisible(SyntaxTokenList modifiers)
    {
        foreach (SyntaxToken modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword))
            {
                return true;
            }
        }

        return false;
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

        foreach (TypeDeclarationSyntax typeDecl in
            context.Root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>())
        {
            // Collect visible property names
            var propertyNames = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                if (member is PropertyDeclarationSyntax prop &&
                    IsVisible(prop.Modifiers))
                {
                    propertyNames.Add(prop.Identifier.Text);
                }
            }

            if (propertyNames.Count == 0)
            {
                continue;
            }

            // Check for Get{PropertyName} methods
            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                if (member is not MethodDeclarationSyntax method ||
                    !IsVisible(method.Modifiers))
                {
                    continue;
                }

                string methodName = method.Identifier.Text;

                if (!methodName.StartsWith(
                    "Get", StringComparison.Ordinal) ||
                    methodName.Length <= 3)
                {
                    continue;
                }

                string suffix = methodName.Substring(3);

                if (propertyNames.Contains(suffix))
                {
                    FileLinePositionSpan span =
                        method.Identifier.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = $"Method '{methodName}' conflicts "
                                + $"with property '{suffix}'",
                            Severity = DefaultSeverity,
                            FilePath = context.FilePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
