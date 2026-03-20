using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ReadonlyFieldOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_readonly_before_mutable";

    public string RuleId => "CSLINT264";

    public string Name => "ReadonlyFieldOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (TypeDeclarationSyntax typeDecl in context.Root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            bool seenMutableInstanceField = false;

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                if (member is not FieldDeclarationSyntax field)
                {
                    continue;
                }

                SyntaxTokenList modifiers = field.Modifiers;

                // Skip const and static fields — they have their own ordering rules
                if (modifiers.Any(SyntaxKind.ConstKeyword) || modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    continue;
                }

                bool isReadonly = modifiers.Any(SyntaxKind.ReadOnlyKeyword);

                if (isReadonly)
                {
                    if (seenMutableInstanceField)
                    {
                        FileLinePositionSpan span = field.Declaration.Variables[0].Identifier.GetLocation().GetLineSpan();

                        (diagnostics ??= []).Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message = "Readonly fields must appear before mutable fields",
                                Severity = LintSeverity.Warning,
                                FilePath = span.Path,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });
                    }
                }
                else
                {
                    seenMutableInstanceField = true;
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
