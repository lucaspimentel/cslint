using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal sealed class CombinedStyleWalker : CSharpSyntaxWalker
{
    private readonly IStyleRuleHandler[] _handlers;

    public CombinedStyleWalker(IStyleRuleHandler[] handlers)
    {
        _handlers = handlers;
    }

    public List<LintDiagnostic> Diagnostics { get; } = [];

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitClassDeclaration(node, Diagnostics);
        }

        base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitStructDeclaration(node, Diagnostics);
        }

        base.VisitStructDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitInterfaceDeclaration(node, Diagnostics);
        }

        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitEnumDeclaration(node, Diagnostics);
        }

        base.VisitEnumDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitMethodDeclaration(node, Diagnostics);
        }

        base.VisitMethodDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitPropertyDeclaration(node, Diagnostics);
        }

        base.VisitPropertyDeclaration(node);
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitFieldDeclaration(node, Diagnostics);
        }

        base.VisitFieldDeclaration(node);
    }

    public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitEventFieldDeclaration(node, Diagnostics);
        }

        base.VisitEventFieldDeclaration(node);
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitLocalDeclarationStatement(node, Diagnostics);
        }

        base.VisitLocalDeclarationStatement(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitIfStatement(node, Diagnostics);
        }

        base.VisitIfStatement(node);
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitForStatement(node, Diagnostics);
        }

        base.VisitForStatement(node);
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitForEachStatement(node, Diagnostics);
        }

        base.VisitForEachStatement(node);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitWhileStatement(node, Diagnostics);
        }

        base.VisitWhileStatement(node);
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitDoStatement(node, Diagnostics);
        }

        base.VisitDoStatement(node);
    }

    public override void VisitUsingStatement(UsingStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitUsingStatement(node, Diagnostics);
        }

        base.VisitUsingStatement(node);
    }

    public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitConditionalExpression(node, Diagnostics);
        }

        base.VisitConditionalExpression(node);
    }
}
