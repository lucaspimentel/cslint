using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class VisibleNonConstStaticFieldRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2211.severity";

    public string RuleId => "CA2211";

    public string Name => "NonConstantFieldsShouldNotBeVisible";

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

        foreach (FieldDeclarationSyntax field in
            context.Root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            // Must be static
            if (!HasModifier(field.Modifiers, SyntaxKind.StaticKeyword))
            {
                continue;
            }

            // Skip const and readonly
            if (HasModifier(field.Modifiers, SyntaxKind.ConstKeyword) ||
                HasModifier(field.Modifiers, SyntaxKind.ReadOnlyKeyword))
            {
                continue;
            }

            // Must be public or protected
            if (!HasModifier(field.Modifiers, SyntaxKind.PublicKeyword) &&
                !HasModifier(field.Modifiers, SyntaxKind.ProtectedKeyword))
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in
                field.Declaration.Variables)
            {
                FileLinePositionSpan span =
                    variable.Identifier.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Non-constant static field "
                            + $"'{variable.Identifier.Text}' should not be "
                            + "visible",
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
