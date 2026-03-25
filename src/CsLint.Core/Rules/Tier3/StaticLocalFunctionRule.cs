using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class StaticLocalFunctionRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "IDE0062";

    public string Name => "StaticLocalFunction";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_prefer_static_local_function"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool CapturesEnclosingState(LocalFunctionStatementSyntax localFunc)
    {
        // Collect names declared in enclosing scopes
        var enclosingNames = new HashSet<string>(StringComparer.Ordinal);
        CollectEnclosingScopeNames(localFunc, enclosingNames);

        if (enclosingNames.Count == 0)
        {
            // No enclosing identifiers to capture — but still check for this/base
            return ContainsThisOrBase(localFunc);
        }

        // Check if the local function body references any enclosing names or this/base
        SyntaxNode? body = (SyntaxNode?)localFunc.Body ?? localFunc.ExpressionBody;

        if (body is null)
        {
            return false;
        }

        // Collect names declared inside the local function (parameters + locals)
        var localNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (ParameterSyntax param in localFunc.ParameterList.Parameters)
        {
            localNames.Add(param.Identifier.Text);
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

        return ContainsThisOrBase(localFunc);
    }

    private static bool ContainsThisOrBase(LocalFunctionStatementSyntax localFunc)
    {
        SyntaxNode? body = (SyntaxNode?)localFunc.Body ?? localFunc.ExpressionBody;

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

    private static void CollectEnclosingScopeNames(
        LocalFunctionStatementSyntax localFunc,
        HashSet<string> names)
    {
        // Walk up the tree to find enclosing method/constructor/accessor and collect declared names
        SyntaxNode? current = localFunc.Parent;

        while (current is not null)
        {
            switch (current)
            {
                case MethodDeclarationSyntax method:
                    AddParameterNames(method.ParameterList, names);
                    CollectLocalNames(method.Body, names, localFunc);
                    CollectLocalNames(method.ExpressionBody, names, localFunc);
                    return;

                case ConstructorDeclarationSyntax ctor:
                    AddParameterNames(ctor.ParameterList, names);
                    CollectLocalNames(ctor.Body, names, localFunc);
                    CollectLocalNames(ctor.ExpressionBody, names, localFunc);
                    return;

                case AccessorDeclarationSyntax accessor:
                    // 'value' is implicitly available in set/init accessors
                    if (accessor.Keyword.IsKind(SyntaxKind.SetKeyword) ||
                        accessor.Keyword.IsKind(SyntaxKind.InitKeyword))
                    {
                        names.Add("value");
                    }

                    CollectLocalNames(accessor.Body, names, localFunc);
                    CollectLocalNames(accessor.ExpressionBody, names, localFunc);
                    return;

                case LocalFunctionStatementSyntax outerLocalFunc:
                    // Enclosing local function — collect its parameters and locals
                    AddParameterNames(outerLocalFunc.ParameterList, names);
                    CollectLocalNames(outerLocalFunc.Body, names, localFunc);
                    CollectLocalNames(outerLocalFunc.ExpressionBody, names, localFunc);
                    // Keep walking up to find more enclosing scopes
                    break;

                case AnonymousFunctionExpressionSyntax:
                    // Lambda/anonymous method boundary — stop here
                    return;
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

    private static void CollectLocalNames(
        SyntaxNode? body,
        HashSet<string> names,
        LocalFunctionStatementSyntax localFunc)
    {
        if (body is null)
        {
            return;
        }

        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            // Stop at the local function itself — don't collect names declared inside it
            if (descendant == localFunc)
            {
                break;
            }

            // Skip nested local functions and lambdas — their declarations aren't in scope
            if (descendant is LocalFunctionStatementSyntax nested && nested != localFunc)
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

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_prefer_static_local_function") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_prefer_static_local_function");

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
        if (node is not LocalFunctionStatementSyntax localFunc)
        {
            return;
        }

        // Already static
        if (localFunc.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_prefer_static_local_function");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Check if the local function captures anything from enclosing scopes
        if (CapturesEnclosingState(localFunc))
        {
            return;
        }

        FileLinePositionSpan span = localFunc.Identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Local function '{localFunc.Identifier.Text}' can be made static",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
