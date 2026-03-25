using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal static class CaptureAnalysis
{
    internal static bool CapturesEnclosingState(SyntaxNode node, SyntaxNode? body, ParameterListSyntax? parameterList)
    {
        // Collect names declared in enclosing scopes
        var enclosingNames = new HashSet<string>(StringComparer.Ordinal);
        CollectEnclosingScopeNames(node, enclosingNames);

        if (enclosingNames.Count == 0)
        {
            return ContainsThisOrBase(body);
        }

        if (body is null)
        {
            return false;
        }

        // Collect names declared inside the node (parameters + locals)
        var localNames = new HashSet<string>(StringComparer.Ordinal);

        if (parameterList is not null)
        {
            foreach (ParameterSyntax param in parameterList.Parameters)
            {
                localNames.Add(param.Identifier.Text);
            }
        }

        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            if (descendant is VariableDeclaratorSyntax varDecl)
            {
                localNames.Add(varDecl.Identifier.Text);
            }
            else if (descendant is SingleVariableDesignationSyntax designation)
            {
                localNames.Add(designation.Identifier.Text);
            }
            else if (descendant is ForEachStatementSyntax forEach)
            {
                localNames.Add(forEach.Identifier.Text);
            }
            else if (descendant is LocalFunctionStatementSyntax nestedFunc)
            {
                localNames.Add(nestedFunc.Identifier.Text);
            }
        }

        // Check for identifier references to enclosing scope names
        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            if (descendant is IdentifierNameSyntax identifier &&
                enclosingNames.Contains(identifier.Identifier.Text) &&
                !localNames.Contains(identifier.Identifier.Text))
            {
                return true;
            }
        }

        return ContainsThisOrBase(body);
    }

    internal static bool CapturesEnclosingState(LocalFunctionStatementSyntax localFunc)
    {
        SyntaxNode? body = (SyntaxNode?)localFunc.Body ?? localFunc.ExpressionBody;
        return CapturesEnclosingState(localFunc, body, localFunc.ParameterList);
    }

    internal static bool CapturesEnclosingState(AnonymousFunctionExpressionSyntax anonFunc)
    {
        SyntaxNode? body = anonFunc.Body;
        ParameterListSyntax? parameterList = anonFunc switch
        {
            ParenthesizedLambdaExpressionSyntax p => p.ParameterList,
            AnonymousMethodExpressionSyntax a => a.ParameterList,
            _ => null,
        };

        // For SimpleLambdaExpressionSyntax, add the single parameter manually
        if (anonFunc is SimpleLambdaExpressionSyntax simple)
        {
            return CapturesEnclosingStateWithExtraParam(anonFunc, body, parameterList, simple.Parameter.Identifier.Text);
        }

        return CapturesEnclosingState(anonFunc, body, parameterList);
    }

    private static bool CapturesEnclosingStateWithExtraParam(
        SyntaxNode node,
        SyntaxNode? body,
        ParameterListSyntax? parameterList,
        string extraParamName)
    {
        var enclosingNames = new HashSet<string>(StringComparer.Ordinal);
        CollectEnclosingScopeNames(node, enclosingNames);

        if (enclosingNames.Count == 0)
        {
            return ContainsThisOrBase(body);
        }

        if (body is null)
        {
            return false;
        }

        var localNames = new HashSet<string>(StringComparer.Ordinal) { extraParamName };

        if (parameterList is not null)
        {
            foreach (ParameterSyntax param in parameterList.Parameters)
            {
                localNames.Add(param.Identifier.Text);
            }
        }

        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            if (descendant is VariableDeclaratorSyntax varDecl)
            {
                localNames.Add(varDecl.Identifier.Text);
            }
            else if (descendant is SingleVariableDesignationSyntax designation)
            {
                localNames.Add(designation.Identifier.Text);
            }
            else if (descendant is ForEachStatementSyntax forEach)
            {
                localNames.Add(forEach.Identifier.Text);
            }
            else if (descendant is LocalFunctionStatementSyntax nestedFunc)
            {
                localNames.Add(nestedFunc.Identifier.Text);
            }
        }

        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            if (descendant is IdentifierNameSyntax identifier &&
                enclosingNames.Contains(identifier.Identifier.Text) &&
                !localNames.Contains(identifier.Identifier.Text))
            {
                return true;
            }
        }

        return ContainsThisOrBase(body);
    }

    private static bool ContainsThisOrBase(SyntaxNode? body)
    {
        if (body is null)
        {
            return false;
        }

        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            if (descendant is ThisExpressionSyntax or BaseExpressionSyntax)
            {
                return true;
            }
        }

        return false;
    }

    private static void CollectEnclosingScopeNames(SyntaxNode node, HashSet<string> names)
    {
        SyntaxNode? current = node.Parent;

        while (current is not null)
        {
            switch (current)
            {
                case MethodDeclarationSyntax method:
                    AddParameterNames(method.ParameterList, names);
                    CollectLocalNames(method.Body, names, node);
                    CollectLocalNames(method.ExpressionBody, names, node);
                    return;

                case ConstructorDeclarationSyntax ctor:
                    AddParameterNames(ctor.ParameterList, names);
                    CollectLocalNames(ctor.Body, names, node);
                    CollectLocalNames(ctor.ExpressionBody, names, node);
                    return;

                case AccessorDeclarationSyntax accessor:
                    if (accessor.Keyword.IsKind(SyntaxKind.SetKeyword) ||
                        accessor.Keyword.IsKind(SyntaxKind.InitKeyword))
                    {
                        names.Add("value");
                    }

                    CollectLocalNames(accessor.Body, names, node);
                    CollectLocalNames(accessor.ExpressionBody, names, node);
                    return;

                case LocalFunctionStatementSyntax outerLocalFunc:
                    AddParameterNames(outerLocalFunc.ParameterList, names);
                    CollectLocalNames(outerLocalFunc.Body, names, node);
                    CollectLocalNames(outerLocalFunc.ExpressionBody, names, node);
                    break;

                case AnonymousFunctionExpressionSyntax outerAnon when outerAnon != node:
                    // Enclosing lambda/anonymous method — collect its parameters
                    switch (outerAnon)
                    {
                        case ParenthesizedLambdaExpressionSyntax p:
                            AddParameterNames(p.ParameterList, names);
                            break;
                        case AnonymousMethodExpressionSyntax a:
                            AddParameterNames(a.ParameterList, names);
                            break;
                        case SimpleLambdaExpressionSyntax s:
                            names.Add(s.Parameter.Identifier.Text);
                            break;
                    }

                    break;
            }

            current = current.Parent;
        }
    }

    private static void AddParameterNames(ParameterListSyntax? parameterList, HashSet<string> names)
    {
        if (parameterList is null)
        {
            return;
        }

        foreach (ParameterSyntax param in parameterList.Parameters)
        {
            names.Add(param.Identifier.Text);
        }
    }

    private static void CollectLocalNames(SyntaxNode? body, HashSet<string> names, SyntaxNode stopAt)
    {
        if (body is null)
        {
            return;
        }

        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            if (descendant == stopAt)
            {
                break;
            }

            if (descendant is LocalFunctionStatementSyntax nested && nested != stopAt)
            {
                continue;
            }

            switch (descendant)
            {
                case VariableDeclaratorSyntax varDecl:
                    names.Add(varDecl.Identifier.Text);
                    break;
                case SingleVariableDesignationSyntax designation:
                    names.Add(designation.Identifier.Text);
                    break;
                case ForEachStatementSyntax forEach:
                    names.Add(forEach.Identifier.Text);
                    break;
            }
        }
    }
}
