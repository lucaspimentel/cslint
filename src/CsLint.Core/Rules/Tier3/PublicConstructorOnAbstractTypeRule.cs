using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PublicConstructorOnAbstractTypeRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1012.severity";

    public string RuleId => "CA1012";

    public string Name => "AbstractTypesShouldNotHavePublicConstructors";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
    {
        foreach (SyntaxToken modifier in modifiers)
        {
            if (modifier.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (TypeDeclarationSyntax typeDecl in context.Root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            if (!HasModifier(typeDecl.Modifiers, SyntaxKind.AbstractKeyword))
            {
                continue;
            }

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                if (member is not ConstructorDeclarationSyntax ctor)
                {
                    continue;
                }

                if (!HasModifier(ctor.Modifiers, SyntaxKind.PublicKeyword))
                {
                    continue;
                }

                FileLinePositionSpan span = ctor.Identifier.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Abstract types should not have public constructors; use protected instead",
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
