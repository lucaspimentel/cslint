using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SealedTypePreferenceRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_prefer_sealed_types";

    public string RuleId => "CSLINT239";

    public string Name => "SealedTypePreference";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    // Force-disabled: without project-wide type hierarchy info, this rule produces
    // false positives on base classes (flags them as "should be sealed" when they
    // have derived types in other files). See TODO.md for the two-pass plan.
    public bool IsEnabled(LintConfiguration configuration) => false;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        switch (node)
        {
            case ClassDeclarationSyntax classDecl:
                CheckTypeDeclaration(classDecl.Modifiers, classDecl.Keyword, filePath, diagnostics);
                break;

            case RecordDeclarationSyntax recordDecl:
                // record struct / record struct Foo — value types, implicitly sealed
                if (recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword))
                {
                    return;
                }

                CheckTypeDeclaration(recordDecl.Modifiers, recordDecl.Keyword, filePath, diagnostics);
                break;
        }
    }

    private void CheckTypeDeclaration(
        SyntaxTokenList modifiers,
        SyntaxToken keyword,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        foreach (SyntaxToken modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.SealedKeyword) ||
                modifier.IsKind(SyntaxKind.AbstractKeyword) ||
                modifier.IsKind(SyntaxKind.StaticKeyword))
            {
                return;
            }
        }

        FileLinePositionSpan span = keyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Type should be sealed",
                Severity = DefaultSeverity,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
