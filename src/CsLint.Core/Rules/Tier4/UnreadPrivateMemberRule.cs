using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class UnreadPrivateMemberRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "IDE0052";

    public string Name => "RemoveUnreadPrivateMember";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.IDE0052.severity"];

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

    private static void CollectPrivateFields(
        SyntaxNode root,
        SemanticModel model,
        Dictionary<ISymbol, SyntaxNode> privateFields)
    {
        foreach (SyntaxNode node in root.DescendantNodes())
        {
            switch (node)
            {
                case FieldDeclarationSyntax field:
                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                    {
                        if (model.GetDeclaredSymbol(variable) is IFieldSymbol symbol &&
                            ShouldCollect(symbol))
                        {
                            privateFields[symbol] = variable;
                        }
                    }

                    break;

                case PropertyDeclarationSyntax property:
                    if (model.GetDeclaredSymbol(property) is IPropertySymbol propSymbol &&
                        propSymbol.DeclaredAccessibility == Accessibility.Private &&
                        !propSymbol.IsImplicitlyDeclared &&
                        !IsInPartialType(propSymbol))
                    {
                        privateFields[propSymbol] = property;
                    }

                    break;

                case EventFieldDeclarationSyntax eventField:
                    foreach (VariableDeclaratorSyntax variable in eventField.Declaration.Variables)
                    {
                        if (model.GetDeclaredSymbol(variable) is IEventSymbol eventSymbol &&
                            eventSymbol.DeclaredAccessibility == Accessibility.Private &&
                            !eventSymbol.IsImplicitlyDeclared &&
                            !IsInPartialType(eventSymbol))
                        {
                            privateFields[eventSymbol] = variable;
                        }
                    }

                    break;
            }
        }
    }

    private static bool ShouldCollect(IFieldSymbol symbol)
    {
        if (symbol.DeclaredAccessibility != Accessibility.Private)
        {
            return false;
        }

        if (symbol.IsImplicitlyDeclared)
        {
            return false;
        }

        // Skip fields named "_" (intentional discards)
        if (symbol.Name == "_")
        {
            return false;
        }

        return !IsInPartialType(symbol);
    }

    private static bool IsInPartialType(ISymbol symbol)
    {
        if (symbol.ContainingType is not { DeclaringSyntaxReferences.Length: > 0 } containingType)
        {
            return false;
        }

        foreach (SyntaxReference syntaxRef in containingType.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is TypeDeclarationSyntax typeDecl &&
                typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return true;
            }
        }

        return false;
    }

    private static void ClassifyReferences(
        SyntaxNode root,
        SemanticModel model,
        Dictionary<ISymbol, SyntaxNode> trackedSymbols,
        HashSet<ISymbol> readSymbols,
        HashSet<ISymbol> writtenSymbols)
    {
        foreach (IdentifierNameSyntax identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            SymbolInfo symbolInfo = model.GetSymbolInfo(identifier);
            ISymbol? referencedSymbol = symbolInfo.Symbol;

            if (referencedSymbol is null)
            {
                continue;
            }

            // Resolve property accessor to containing property
            if (referencedSymbol is IMethodSymbol { AssociatedSymbol: not null } accessor)
            {
                referencedSymbol = accessor.AssociatedSymbol;
            }

            if (!trackedSymbols.ContainsKey(referencedSymbol))
            {
                continue;
            }

            if (IsWriteReference(identifier))
            {
                writtenSymbols.Add(referencedSymbol);
            }
            else
            {
                readSymbols.Add(referencedSymbol);
            }
        }
    }

    private static bool IsWriteReference(IdentifierNameSyntax identifier)
    {
        SyntaxNode? parent = identifier.Parent;

        // Simple assignment: x = value
        if (parent is AssignmentExpressionSyntax assignment &&
            assignment.Left == identifier)
        {
            // Compound assignments (+=, -=, etc.) are both read and write
            return assignment.Kind() == SyntaxKind.SimpleAssignmentExpression;
        }

        // Prefix/postfix increment/decrement: x++, ++x, x--, --x (read + write)
        if (parent is PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax)
        {
            return false; // These are reads too
        }

        // out argument: Method(out x) — this is a write
        if (parent is ArgumentSyntax argument)
        {
            return argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);
        }

        return false;
    }

    private static SyntaxToken GetIdentifier(SyntaxNode node) =>
        node switch
        {
            VariableDeclaratorSyntax v => v.Identifier,
            PropertyDeclarationSyntax p => p.Identifier,
            _ => default,
        };

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.IDE0052.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        if (HasMissingReferences(model))
        {
            return;
        }

        var privateFields = new Dictionary<ISymbol, SyntaxNode>(SymbolEqualityComparer.Default);
        CollectPrivateFields(context.Root, model, privateFields);

        if (privateFields.Count == 0)
        {
            return;
        }

        var readFields = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var writtenFields = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        ClassifyReferences(context.Root, model, privateFields, readFields, writtenFields);

        // Report fields that are written but never read (IDE0052).
        // Fields that are never referenced at all are IDE0051's responsibility.
        foreach ((ISymbol symbol, SyntaxNode node) in privateFields)
        {
            if (writtenFields.Contains(symbol) && !readFields.Contains(symbol))
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
                    Message = $"Private member '{symbol.Name}' can be removed as the value assigned to it is never read",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = start.Line + 1,
                    Column = start.Character + 1,
                });
            }
        }
    }
}
