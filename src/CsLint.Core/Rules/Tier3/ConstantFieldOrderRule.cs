using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConstantFieldOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_constants_before_fields";

    public string RuleId => "CSLINT265";

    public string Name => "ConstantFieldOrder";

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
            bool seenNonConstField = false;

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                if (member is not FieldDeclarationSyntax field)
                {
                    continue;
                }

                bool isConst = field.Modifiers.Any(SyntaxKind.ConstKeyword);

                if (isConst)
                {
                    if (seenNonConstField)
                    {
                        FileLinePositionSpan span = field.Declaration.Variables[0].Identifier.GetLocation().GetLineSpan();

                        (diagnostics ??= []).Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message = "Constant fields must appear before non-constant fields",
                                Severity = LintSeverity.Warning,
                                FilePath = span.Path,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });
                    }
                }
                else
                {
                    seenNonConstField = true;
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
