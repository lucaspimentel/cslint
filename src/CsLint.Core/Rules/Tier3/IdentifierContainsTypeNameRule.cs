using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class IdentifierContainsTypeNameRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1720.severity";

    public string RuleId => "CA1720";

    public string Name => "IdentifiersShouldNotContainTypeNames";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    // Type names that should not appear as standalone parameter/member names
    private static readonly HashSet<string> TypeNames = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "bool", "byte", "char", "decimal", "double", "float",
        "int", "long", "object", "sbyte", "short", "string",
        "uint", "ulong", "ushort",
        "int16", "int32", "int64", "uint16", "uint32", "uint64",
        "single", "boolean",
    };

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        // Check parameters of public methods
        foreach (MethodDeclarationSyntax method in
            context.Root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>())
        {
            if (!IsVisible(method.Modifiers))
            {
                continue;
            }

            foreach (ParameterSyntax param in method.ParameterList.Parameters)
            {
                if (TypeNames.Contains(param.Identifier.Text))
                {
                    Report(
                        param.Identifier, context.FilePath,
                        ref diagnostics);
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

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

    private void Report(
        SyntaxToken identifier,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

        (diagnostics ??= []).Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Parameter '{identifier.Text}' should not be "
                    + "named after a type",
                Severity = DefaultSeverity,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
