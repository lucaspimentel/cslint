using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class EnumStorageShouldBeInt32Rule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1028.severity";

    public string RuleId => "CA1028";

    public string Name => "EnumStorageShouldBeInt32";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static readonly HashSet<SyntaxKind> AllowedBaseTypes =
    [
        SyntaxKind.IntKeyword, // int (default)
    ];

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
            if (enumDecl.BaseList is null)
            {
                continue; // defaults to int
            }

            // The base type is the first (and only) type in the base list
            BaseTypeSyntax? baseType = enumDecl.BaseList.Types.FirstOrDefault();

            if (baseType is null)
            {
                continue;
            }

            // Check if the base type is a predefined type keyword
            if (baseType.Type is PredefinedTypeSyntax predefined &&
                AllowedBaseTypes.Contains(predefined.Keyword.Kind()))
            {
                continue; // explicit `: int` is fine
            }

            FileLinePositionSpan span =
                enumDecl.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Enum '{enumDecl.Identifier.Text}' should use "
                        + "Int32 as its underlying type",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
