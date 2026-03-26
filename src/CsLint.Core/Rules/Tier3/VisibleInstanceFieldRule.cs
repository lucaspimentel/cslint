using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class VisibleInstanceFieldRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1051.severity";

    public string RuleId => "CA1051";

    public string Name => "DoNotDeclareVisibleInstanceFields";

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

    private static bool IsVisibleInstanceField(FieldDeclarationSyntax field)
    {
        // Skip static and const fields
        if (HasModifier(field.Modifiers, SyntaxKind.StaticKeyword) ||
            HasModifier(field.Modifiers, SyntaxKind.ConstKeyword))
        {
            return false;
        }

        // Skip fields in structs — public fields are standard practice for structs
        if (field.Parent is StructDeclarationSyntax or RecordDeclarationSyntax { ClassOrStructKeyword.RawKind: (int)SyntaxKind.StructKeyword })
        {
            return false;
        }

        // Must be public or protected
        return HasModifier(field.Modifiers, SyntaxKind.PublicKeyword) ||
            HasModifier(field.Modifiers, SyntaxKind.ProtectedKeyword);
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
            if (!IsVisibleInstanceField(field))
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                FileLinePositionSpan span =
                    variable.Identifier.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Do not declare visible instance field "
                            + $"'{variable.Identifier.Text}'",
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
