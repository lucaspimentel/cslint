using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class TypeOutsideNamespaceRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1050.severity";

    public string RuleId => "CA1050";

    public string Name => "DeclareTypesInNamespaces";

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

        if (context.Root is not CompilationUnitSyntax compilationUnit)
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (MemberDeclarationSyntax member in compilationUnit.Members)
        {
            // Skip namespace declarations and global statements
            SyntaxToken? identifier = member switch
            {
                TypeDeclarationSyntax td => td.Identifier,
                EnumDeclarationSyntax ed => ed.Identifier,
                DelegateDeclarationSyntax dd => dd.Identifier,
                _ => null,
            };

            if (identifier is null)
            {
                continue;
            }

            FileLinePositionSpan span =
                identifier.Value.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Declare types in namespaces",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
