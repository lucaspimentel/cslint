using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class FieldsMustBePrivateRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_fields_must_be_private";

    public string RuleId => "SA1401";

    public string Name => "FieldsMustBePrivate";

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

    private static bool HasAnyAccessModifier(SyntaxTokenList modifiers)
    {
        foreach (SyntaxToken modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.InternalKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitFieldDeclaration(
        FieldDeclarationSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Skip const and static readonly fields — these are commonly non-private
        if (HasModifier(node.Modifiers, SyntaxKind.ConstKeyword))
        {
            return;
        }

        if (HasModifier(node.Modifiers, SyntaxKind.StaticKeyword) &&
            HasModifier(node.Modifiers, SyntaxKind.ReadOnlyKeyword))
        {
            return;
        }

        // Skip struct fields — public fields are common and accepted in structs
        // (value-type data carriers, interop, etc.)
        if (node.Parent is StructDeclarationSyntax)
        {
            return;
        }

        // Skip private fields (including implicitly private — no access modifier)
        if (HasModifier(node.Modifiers, SyntaxKind.PrivateKeyword))
        {
            return;
        }

        // If no access modifier, it's implicitly private
        if (!HasAnyAccessModifier(node.Modifiers))
        {
            return;
        }

        FileLinePositionSpan span = node.Declaration.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Field should be private",
                Severity = DefaultSeverity,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
