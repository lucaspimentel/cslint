using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SystemThreadingLockRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "IDE0330";

    public string Name => "SystemThreadingLock";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_prefer_system_threading_lock"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsObjectType(TypeSyntax type) =>
        type is PredefinedTypeSyntax predefined && predefined.Keyword.IsKind(SyntaxKind.ObjectKeyword);

    private static TypeDeclarationSyntax? FindEnclosingType(SyntaxNode node)
    {
        SyntaxNode? current = node.Parent;

        while (current is not null)
        {
            if (current is TypeDeclarationSyntax typeDecl)
            {
                return typeDecl;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool IsObjectField(string fieldName, TypeDeclarationSyntax enclosingType)
    {
        foreach (MemberDeclarationSyntax member in enclosingType.Members)
        {
            if (member is not FieldDeclarationSyntax field)
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                if (variable.Identifier.Text == fieldName && IsObjectType(field.Declaration.Type))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_prefer_system_threading_lock") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_prefer_system_threading_lock");

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
        if (node is not LockStatementSyntax lockStatement)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_prefer_system_threading_lock");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ExpressionSyntax expression = lockStatement.Expression;
        string? message = GetDiagnosticMessage(expression);

        if (message is null)
        {
            return;
        }

        FileLinePositionSpan span = lockStatement.LockKeyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = message,
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private string? GetDiagnosticMessage(ExpressionSyntax expression)
    {
        // lock (this)
        if (expression is ThisExpressionSyntax)
        {
            return "Use a dedicated System.Threading.Lock field instead of locking on 'this'";
        }

        // lock (typeof(...))
        if (expression is TypeOfExpressionSyntax)
        {
            return "Use a dedicated System.Threading.Lock field instead of locking on a Type object";
        }

        // Extract field name from identifier or this.identifier
        string? fieldName = expression switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: IdentifierNameSyntax name } => name.Identifier.Text,
            _ => null,
        };

        if (fieldName is null)
        {
            return null;
        }

        TypeDeclarationSyntax? enclosingType = FindEnclosingType(expression);

        if (enclosingType is null)
        {
            return null;
        }

        if (IsObjectField(fieldName, enclosingType))
        {
            return $"Use System.Threading.Lock instead of object for lock field '{fieldName}'";
        }

        return null;
    }
}
