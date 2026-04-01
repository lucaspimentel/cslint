using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class IdentifierMatchesKeywordRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1716.severity";

    public string RuleId => "CA1716";

    public string Name => "IdentifiersShouldNotMatchKeywords";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    // C# reserved keywords (subset most likely to conflict with identifiers)
    private static readonly HashSet<string> Keywords = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case",
        "catch", "char", "checked", "class", "const", "continue",
        "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally",
        "fixed", "float", "for", "foreach", "goto", "if", "implicit",
        "in", "int", "interface", "internal", "is", "lock", "long",
        "namespace", "new", "null", "object", "operator", "out",
        "override", "params", "private", "protected", "public",
        "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct",
        "switch", "this", "throw", "true", "try", "typeof", "uint",
        "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
        "void", "volatile", "while",
    };

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

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            switch (node)
            {
                // Namespaces, types, methods, properties
                case NamespaceDeclarationSyntax ns
                    when Keywords.Contains(GetLastIdentifier(ns.Name)):
                    ReportName(
                        GetLastIdentifier(ns.Name),
                        ns.Name.GetLocation(),
                        context.FilePath,
                        ref diagnostics);
                    break;

                case TypeDeclarationSyntax type
                    when IsVisible(type.Modifiers) &&
                        Keywords.Contains(type.Identifier.Text):
                    Report(
                        type.Identifier, context.FilePath,
                        ref diagnostics);
                    break;

                case MethodDeclarationSyntax method
                    when IsVisible(method.Modifiers) &&
                        Keywords.Contains(method.Identifier.Text):
                    Report(
                        method.Identifier, context.FilePath,
                        ref diagnostics);
                    break;

                case PropertyDeclarationSyntax property
                    when IsVisible(property.Modifiers) &&
                        Keywords.Contains(property.Identifier.Text):
                    Report(
                        property.Identifier, context.FilePath,
                        ref diagnostics);
                    break;
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private static string GetLastIdentifier(NameSyntax name) =>
        name switch
        {
            QualifiedNameSyntax q => q.Right.Identifier.Text,
            IdentifierNameSyntax id => id.Identifier.Text,
            _ => string.Empty,
        };

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
                Message = $"Identifier '{identifier.Text}' should not "
                    + "match a language keyword",
                Severity = DefaultSeverity,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private void ReportName(
        string name,
        Location location,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        FileLinePositionSpan span = location.GetLineSpan();

        (diagnostics ??= []).Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Identifier '{name}' should not match a "
                    + "language keyword",
                Severity = DefaultSeverity,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
