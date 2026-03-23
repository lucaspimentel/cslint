using Cslint.Core.Config;
using Cslint.Core.Rules.Tier3;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class NamingConventionRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "IDE1006";

    public string Name => "NamingConvention";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public static bool HasStandardNamingConfig(LintConfiguration configuration)
    {
        foreach (string key in configuration.Properties.Keys)
        {
            if (key.StartsWith("dotnet_naming_rule.", StringComparison.OrdinalIgnoreCase)
                && key.EndsWith(".symbols", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetSymbolKind(SyntaxNode node) =>
        node switch
        {
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            InterfaceDeclarationSyntax => "interface",
            EnumDeclarationSyntax => "enum",
            RecordDeclarationSyntax => "class",
            DelegateDeclarationSyntax => "delegate",
            MethodDeclarationSyntax => "method",
            PropertyDeclarationSyntax => "property",
            EventDeclarationSyntax or EventFieldDeclarationSyntax => "event",
            FieldDeclarationSyntax => "field",
            ParameterSyntax => "parameter",
            TypeParameterSyntax => "type_parameter",
            LocalDeclarationStatementSyntax => "local",
            LocalFunctionStatementSyntax => "local_function",
            BaseNamespaceDeclarationSyntax => "namespace",
            _ => "",
        };

    private static string GetAccessibility(SyntaxTokenList modifiers)
    {
        bool hasPublic = modifiers.Any(SyntaxKind.PublicKeyword);
        bool hasPrivate = modifiers.Any(SyntaxKind.PrivateKeyword);
        bool hasProtected = modifiers.Any(SyntaxKind.ProtectedKeyword);
        bool hasInternal = modifiers.Any(SyntaxKind.InternalKeyword);

        if (hasPublic)
        {
            return "public";
        }

        if (hasPrivate && hasProtected)
        {
            return "private_protected";
        }

        if (hasProtected && hasInternal)
        {
            return "protected_internal";
        }

        if (hasProtected)
        {
            return "protected";
        }

        if (hasInternal)
        {
            return "internal";
        }

        if (hasPrivate)
        {
            return "private";
        }

        return "private"; // Default for members without explicit access
    }

    private static bool HasModifier(SyntaxTokenList modifiers, string modifier) =>
        modifier.ToLowerInvariant() switch
        {
            "abstract" => modifiers.Any(SyntaxKind.AbstractKeyword),
            "async" => modifiers.Any(SyntaxKind.AsyncKeyword),
            "const" => modifiers.Any(SyntaxKind.ConstKeyword),
            "readonly" => modifiers.Any(SyntaxKind.ReadOnlyKeyword),
            // const is implicitly static per the standard
            "static" => modifiers.Any(SyntaxKind.StaticKeyword)
                || modifiers.Any(SyntaxKind.ConstKeyword),
            _ => false,
        };

    private static bool MatchesSymbolGroup(
        string kind,
        string accessibility,
        SyntaxTokenList modifiers,
        NamingSymbolGroup group)
    {
        // Check applicable_kinds
        if (!group.ApplicableKinds.Contains("*") && !group.ApplicableKinds.Contains(kind))
        {
            return false;
        }

        // Check applicable_accessibilities
        if (!group.ApplicableAccessibilities.Contains("*")
            && !group.ApplicableAccessibilities.Contains(accessibility))
        {
            return false;
        }

        // Check required_modifiers (all must be present)
        foreach (string requiredModifier in group.RequiredModifiers)
        {
            if (!HasModifier(modifiers, requiredModifier))
            {
                return false;
            }
        }

        return true;
    }

    private static (string? identifier, SyntaxTokenList modifiers, Location? location)
        GetIdentifierInfo(SyntaxNode node) =>
        node switch
        {
            TypeDeclarationSyntax t => (t.Identifier.Text, t.Modifiers, t.Identifier.GetLocation()),
            DelegateDeclarationSyntax d => (d.Identifier.Text, d.Modifiers, d.Identifier.GetLocation()),
            MethodDeclarationSyntax m => (m.Identifier.Text, m.Modifiers, m.Identifier.GetLocation()),
            PropertyDeclarationSyntax p => (p.Identifier.Text, p.Modifiers, p.Identifier.GetLocation()),
            EventDeclarationSyntax e => (e.Identifier.Text, e.Modifiers, e.Identifier.GetLocation()),
            FieldDeclarationSyntax f when f.Declaration.Variables.Count == 1 =>
                (f.Declaration.Variables[0].Identifier.Text, f.Modifiers,
                    f.Declaration.Variables[0].Identifier.GetLocation()),
            EventFieldDeclarationSyntax ef when ef.Declaration.Variables.Count == 1 =>
                (ef.Declaration.Variables[0].Identifier.Text, ef.Modifiers,
                    ef.Declaration.Variables[0].Identifier.GetLocation()),
            ParameterSyntax param => (param.Identifier.Text, default, param.Identifier.GetLocation()),
            TypeParameterSyntax tp => (tp.Identifier.Text, default, tp.Identifier.GetLocation()),
            LocalDeclarationStatementSyntax local when local.Declaration.Variables.Count == 1 =>
                (local.Declaration.Variables[0].Identifier.Text, local.Modifiers,
                    local.Declaration.Variables[0].Identifier.GetLocation()),
            LocalFunctionStatementSyntax lf =>
                (lf.Identifier.Text, lf.Modifiers, lf.Identifier.GetLocation()),
            BaseNamespaceDeclarationSyntax ns =>
                (ns.Name.ToString(), default, ns.Name.GetLocation()),
            _ => (null, default, null),
        };

    private static string DescribeStyle(NamingStyleDefinition style)
    {
        string desc = style.Capitalization;

        if (style.RequiredPrefix is not null)
        {
            desc = $"'{style.RequiredPrefix}' prefix + {desc}";
        }

        if (style.RequiredSuffix is not null)
        {
            desc += $" + '{style.RequiredSuffix}' suffix";
        }

        return desc;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        HasStandardNamingConfig(configuration);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<ParsedNamingRule> rules = NamingConventionParser.Parse(
            context.Configuration.Properties);

        if (rules.Count == 0)
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
        // Extract identifier and modifiers from declaration nodes
        (string? identifier, SyntaxTokenList modifiers, Location? location) =
            GetIdentifierInfo(node);

        if (identifier is null || location is null)
        {
            return;
        }

        string kind = GetSymbolKind(node);

        if (kind.Length == 0)
        {
            return;
        }

        string accessibility = kind is "parameter" or "local" or "type_parameter" or "local_function"
            ? "local"
            : GetAccessibility(modifiers);

        List<ParsedNamingRule> rules = NamingConventionParser.Parse(config.Properties);

        foreach (ParsedNamingRule rule in rules)
        {
            if (!MatchesSymbolGroup(kind, accessibility, modifiers, rule.Symbols))
            {
                continue;
            }

            // First matching rule wins
            if (!NamingConventionParser.MatchesStyle(identifier, rule.Style))
            {
                FileLinePositionSpan span = location.GetLineSpan();
                string expected = DescribeStyle(rule.Style);

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Naming rule violation: '{identifier}' should be {expected}",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }

            break; // First match wins
        }
    }
}
