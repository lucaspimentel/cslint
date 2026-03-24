using System.Text.RegularExpressions;
using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed partial class ExplicitTupleNamesRule : IRuleDefinition
{
    public string RuleId => "IDE0033";

    public string Name => "ExplicitTupleNames";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_explicit_tuple_names"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    [GeneratedRegex(@"^Item\d+$")]
    private static partial Regex ItemNPattern();

    private static string? FindTupleElementName(
        SyntaxNode accessNode,
        string variableName,
        int elementIndex)
    {
        // Walk up to find enclosing method/property body
        SyntaxNode? current = accessNode.Parent;

        while (current is not null)
        {
            // Check local variable declarations
            foreach (SyntaxNode descendant in current.DescendantNodes())
            {
                // Stop searching at the access node — only look at declarations before it
                if (descendant.SpanStart >= accessNode.SpanStart)
                {
                    break;
                }

                if (descendant is VariableDeclaratorSyntax declarator &&
                    declarator.Identifier.Text == variableName &&
                    declarator.Parent is VariableDeclarationSyntax declaration &&
                    declaration.Type is TupleTypeSyntax tupleType)
                {
                    return GetNamedElementAt(tupleType, elementIndex);
                }

                if (descendant is SingleVariableDesignationSyntax designation &&
                    designation.Identifier.Text == variableName &&
                    designation.Parent is DeclarationExpressionSyntax declExpr &&
                    declExpr.Type is TupleTypeSyntax declTupleType)
                {
                    return GetNamedElementAt(declTupleType, elementIndex);
                }

                if (descendant is ForEachStatementSyntax forEach &&
                    forEach.Identifier.Text == variableName &&
                    forEach.Type is TupleTypeSyntax forEachTupleType)
                {
                    return GetNamedElementAt(forEachTupleType, elementIndex);
                }
            }

            // Check parameters of enclosing method/local function
            if (current is MethodDeclarationSyntax method)
            {
                string? name = FindInParameters(method.ParameterList, variableName, elementIndex);

                if (name is not null)
                {
                    return name;
                }

                break; // Don't search beyond the enclosing method
            }

            if (current is LocalFunctionStatementSyntax localFunc)
            {
                string? name = FindInParameters(localFunc.ParameterList, variableName, elementIndex);

                if (name is not null)
                {
                    return name;
                }
            }

            if (current is ConstructorDeclarationSyntax ctor)
            {
                string? name = FindInParameters(ctor.ParameterList, variableName, elementIndex);

                if (name is not null)
                {
                    return name;
                }

                break;
            }

            if (current is AccessorDeclarationSyntax)
            {
                break;
            }

            current = current.Parent;
        }

        return null;
    }

    private static string? FindInParameters(
        ParameterListSyntax? parameterList,
        string variableName,
        int elementIndex)
    {
        if (parameterList is null)
        {
            return null;
        }

        foreach (ParameterSyntax parameter in parameterList.Parameters)
        {
            if (parameter.Identifier.Text == variableName &&
                parameter.Type is TupleTypeSyntax tupleType)
            {
                return GetNamedElementAt(tupleType, elementIndex);
            }
        }

        return null;
    }

    private static string? GetNamedElementAt(TupleTypeSyntax tupleType, int index)
    {
        if (index < 0 || index >= tupleType.Elements.Count)
        {
            return null;
        }

        TupleElementSyntax element = tupleType.Elements[index];

        // Only return the name if the element has an explicit identifier
        return element.Identifier.IsKind(SyntaxKind.None) ? null : element.Identifier.Text;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_explicit_tuple_names") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("dotnet_style_explicit_tuple_names");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            if (node is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            string memberName = memberAccess.Name.Identifier.Text;

            if (!ItemNPattern().IsMatch(memberName))
            {
                continue;
            }

            if (memberAccess.Expression is not IdentifierNameSyntax identifierName)
            {
                continue;
            }

            string variableName = identifierName.Identifier.Text;
            int itemIndex = int.Parse(memberName.AsSpan(4)) - 1; // "Item1" → index 0

            string? suggestedName = FindTupleElementName(memberAccess, variableName, itemIndex);

            if (suggestedName is null)
            {
                continue;
            }

            FileLinePositionSpan span = memberAccess.Name.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Use '{suggestedName}' instead of '{memberName}'",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics;
    }
}
