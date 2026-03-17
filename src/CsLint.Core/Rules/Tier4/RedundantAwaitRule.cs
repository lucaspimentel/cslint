using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class RedundantAwaitRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "CSLINT307";

    public string Name => "RedundantAwait";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.CSLINT307.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetSeverityForKey("dotnet_diagnostic.CSLINT307.severity") != LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            switch (node)
            {
                case MethodDeclarationSyntax method when HasAsyncModifier(method.Modifiers):
                    CheckMethodLike(method.Body, method.ExpressionBody, method.ReturnType, method.Identifier.Text, context, model, diagnostics);
                    break;

                case LocalFunctionStatementSyntax localFunc when HasAsyncModifier(localFunc.Modifiers):
                    CheckMethodLike(
                        localFunc.Body, localFunc.ExpressionBody, localFunc.ReturnType,
                        localFunc.Identifier.Text, context, model, diagnostics);
                    break;

                case ParenthesizedLambdaExpressionSyntax lambda when HasAsyncModifier(lambda.Modifiers):
                    CheckLambda(lambda.Block, lambda.ExpressionBody, lambda, context, model, diagnostics);
                    break;

                case SimpleLambdaExpressionSyntax lambda when HasAsyncModifier(lambda.Modifiers):
                    CheckLambda(lambda.Block, lambda.ExpressionBody, lambda, context, model, diagnostics);
                    break;
            }
        }
    }

    private static bool HasAsyncModifier(SyntaxTokenList modifiers) =>
        modifiers.Any(SyntaxKind.AsyncKeyword);

    private void CheckMethodLike(
        BlockSyntax? body,
        ArrowExpressionClauseSyntax? expressionBody,
        TypeSyntax returnType,
        string name,
        RuleContext context,
        SemanticModel model,
        List<LintDiagnostic> diagnostics)
    {
        AwaitExpressionSyntax? awaitExpr = GetSingleAwait(body, expressionBody);

        if (awaitExpr is null)
        {
            return;
        }

        if (HasConfigureAwait(awaitExpr))
        {
            return;
        }

        ITypeSymbol? returnTypeSymbol = model.GetTypeInfo(returnType).Type;

        if (returnTypeSymbol is null || returnTypeSymbol.TypeKind == TypeKind.Error)
        {
            return;
        }

        if (IsValueTask(returnTypeSymbol))
        {
            return;
        }

        ReportDiagnostic(awaitExpr, name, context.FilePath, diagnostics);
    }

    private void CheckLambda(
        BlockSyntax? body,
        ExpressionSyntax? expressionBody,
        LambdaExpressionSyntax lambda,
        RuleContext context,
        SemanticModel model,
        List<LintDiagnostic> diagnostics)
    {
        AwaitExpressionSyntax? awaitExpr = null;

        if (body is not null)
        {
            awaitExpr = GetSingleAwaitFromBlock(body);
        }
        else if (expressionBody is AwaitExpressionSyntax awaitBody)
        {
            awaitExpr = awaitBody;
        }

        if (awaitExpr is null)
        {
            return;
        }

        if (HasConfigureAwait(awaitExpr))
        {
            return;
        }

        TypeInfo typeInfo = model.GetTypeInfo(lambda);
        ITypeSymbol? convertedType = typeInfo.ConvertedType;

        if (convertedType is INamedTypeSymbol { DelegateInvokeMethod.ReturnType: { } delegateReturnType })
        {
            if (delegateReturnType.TypeKind == TypeKind.Error)
            {
                return;
            }

            if (IsValueTask(delegateReturnType))
            {
                return;
            }
        }

        ReportDiagnostic(awaitExpr, "lambda", context.FilePath, diagnostics);
    }

    private static AwaitExpressionSyntax? GetSingleAwait(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody)
    {
        if (body is not null)
        {
            return GetSingleAwaitFromBlock(body);
        }

        if (expressionBody?.Expression is AwaitExpressionSyntax awaitExpr)
        {
            return awaitExpr;
        }

        return null;
    }

    private static AwaitExpressionSyntax? GetSingleAwaitFromBlock(BlockSyntax body)
    {
        if (body.Statements.Count != 1)
        {
            return null;
        }

        if (body.Statements[0] is not ReturnStatementSyntax { Expression: AwaitExpressionSyntax awaitExpr })
        {
            return null;
        }

        // Skip if the block contains try/catch/finally or using statements
        if (body.DescendantNodes().Any(
                n => n is TryStatementSyntax
                    or UsingStatementSyntax
                    or LocalDeclarationStatementSyntax { UsingKeyword.RawKind: not 0 }))
        {
            return null;
        }

        return awaitExpr;
    }

    private static bool HasConfigureAwait(AwaitExpressionSyntax awaitExpr) =>
        awaitExpr.Expression is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "ConfigureAwait" }
        };

    private static bool IsValueTask(ITypeSymbol type) =>
        type.Name == "ValueTask" &&
        type.ContainingNamespace is { Name: "Tasks", ContainingNamespace.Name: "Threading", ContainingNamespace.ContainingNamespace.Name: "System" };

    private void ReportDiagnostic(AwaitExpressionSyntax awaitExpr, string memberName, string filePath, List<LintDiagnostic> diagnostics)
    {
        LinePosition start = awaitExpr.AwaitKeyword.GetLocation().GetLineSpan().StartLinePosition;

        diagnostics.Add(new LintDiagnostic
        {
            RuleId = RuleId,
            Message = $"Redundant await in '{memberName}'; return the task directly",
            Severity = DefaultSeverity,
            FilePath = filePath,
            Line = start.Line + 1,
            Column = start.Character + 1,
        });
    }
}
