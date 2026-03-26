using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class UnusedPrivateMemberRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "IDE0051";

    public string Name => "RemoveUnusedPrivateMember";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.IDE0051.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasMissingReferences(SemanticModel model)
    {
        foreach (Diagnostic diagnostic in model.GetDiagnostics())
        {
            if (diagnostic.Id is "CS0246" or "CS0234")
            {
                return true;
            }
        }

        return false;
    }

    private static void CollectPrivateMembers(
        SyntaxNode root,
        SemanticModel model,
        Dictionary<ISymbol, SyntaxNode> privateMembers)
    {
        foreach (SyntaxNode node in root.DescendantNodes())
        {
            switch (node)
            {
                case FieldDeclarationSyntax field:
                    CollectFieldMembers(field, model, privateMembers);
                    break;

                case MethodDeclarationSyntax method:
                    CollectMethodMember(method, model, privateMembers);
                    break;

                case PropertyDeclarationSyntax property:
                    CollectSimpleMember(property, model, privateMembers);
                    break;

                case EventDeclarationSyntax eventDecl:
                    CollectSimpleMember(eventDecl, model, privateMembers);
                    break;

                case EventFieldDeclarationSyntax eventField:
                    CollectEventFieldMembers(eventField, model, privateMembers);
                    break;
            }
        }
    }

    private static void CollectFieldMembers(
        FieldDeclarationSyntax field,
        SemanticModel model,
        Dictionary<ISymbol, SyntaxNode> privateMembers)
    {
        foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
        {
            if (model.GetDeclaredSymbol(variable) is not IFieldSymbol symbol)
            {
                continue;
            }

            if (!ShouldCollect(symbol, variable))
            {
                continue;
            }

            // Skip fields named "_" (intentional discards)
            if (symbol.Name == "_")
            {
                continue;
            }

            privateMembers[symbol] = variable;
        }
    }

    private static void CollectMethodMember(
        MethodDeclarationSyntax method,
        SemanticModel model,
        Dictionary<ISymbol, SyntaxNode> privateMembers)
    {
        if (model.GetDeclaredSymbol(method) is not IMethodSymbol symbol)
        {
            return;
        }

        if (!ShouldCollect(symbol, method))
        {
            return;
        }

        // Skip entry point methods
        if (symbol.Name is "Main" or "<Main>$")
        {
            return;
        }

        // Skip override and extern methods
        if (symbol.IsOverride || symbol.IsExtern)
        {
            return;
        }

        privateMembers[symbol] = method;
    }

    private static void CollectSimpleMember(
        MemberDeclarationSyntax member,
        SemanticModel model,
        Dictionary<ISymbol, SyntaxNode> privateMembers)
    {
        if (model.GetDeclaredSymbol(member) is not ISymbol symbol)
        {
            return;
        }

        if (!ShouldCollect(symbol, member))
        {
            return;
        }

        privateMembers[symbol] = member;
    }

    private static void CollectEventFieldMembers(
        EventFieldDeclarationSyntax eventField,
        SemanticModel model,
        Dictionary<ISymbol, SyntaxNode> privateMembers)
    {
        foreach (VariableDeclaratorSyntax variable in eventField.Declaration.Variables)
        {
            if (model.GetDeclaredSymbol(variable) is not IEventSymbol symbol)
            {
                continue;
            }

            if (!ShouldCollect(symbol, variable))
            {
                continue;
            }

            privateMembers[symbol] = variable;
        }
    }

    private static bool ShouldCollect(ISymbol symbol, SyntaxNode node)
    {
        // Only flag private members
        if (symbol.DeclaredAccessibility != Accessibility.Private)
        {
            return false;
        }

        // Skip implicitly declared members
        if (symbol.IsImplicitlyDeclared)
        {
            return false;
        }

        // Skip members in partial types (could be referenced from another file)
        if (symbol.ContainingType is { DeclaringSyntaxReferences.Length: > 0 } containingType)
        {
            foreach (SyntaxReference syntaxRef in containingType.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax() is TypeDeclarationSyntax typeDecl &&
                    typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static SyntaxToken GetIdentifier(SyntaxNode node) =>
        node switch
        {
            VariableDeclaratorSyntax v => v.Identifier,
            MethodDeclarationSyntax m => m.Identifier,
            PropertyDeclarationSyntax p => p.Identifier,
            EventDeclarationSyntax e => e.Identifier,
            _ => default,
        };

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.IDE0051.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        // Bail out if compilation has missing references — false positive mitigation
        if (HasMissingReferences(model))
        {
            return;
        }

        var privateMembers = new Dictionary<ISymbol, SyntaxNode>(SymbolEqualityComparer.Default);

        CollectPrivateMembers(context.Root, model, privateMembers);

        if (privateMembers.Count == 0)
        {
            return;
        }

        // Walk all identifier references and remove matched symbols
        foreach (IdentifierNameSyntax identifier in context.Root.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            SymbolInfo symbolInfo = model.GetSymbolInfo(identifier);
            ISymbol? referencedSymbol = symbolInfo.Symbol;

            if (referencedSymbol is null)
            {
                // Check candidates (e.g. overload resolution ambiguity)
                foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
                {
                    privateMembers.Remove(candidate);
                }

                continue;
            }

            // For properties accessed via accessors, resolve to the containing property
            if (referencedSymbol is IMethodSymbol { AssociatedSymbol: not null } accessor)
            {
                privateMembers.Remove(accessor.AssociatedSymbol);
            }

            privateMembers.Remove(referencedSymbol);

            if (privateMembers.Count == 0)
            {
                return;
            }
        }

        // Report remaining unused members
        foreach ((ISymbol symbol, SyntaxNode node) in privateMembers)
        {
            SyntaxToken identifier = GetIdentifier(node);

            if (identifier == default)
            {
                continue;
            }

            LinePosition start = identifier.GetLocation().GetLineSpan().StartLinePosition;

            diagnostics.Add(new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Private member '{symbol.Name}' is declared but never used",
                Severity = DefaultSeverity,
                FilePath = context.FilePath,
                Line = start.Line + 1,
                Column = start.Character + 1,
            });
        }
    }
}
