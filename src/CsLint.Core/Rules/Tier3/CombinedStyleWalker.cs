using Cslint.Core.Config;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal sealed class CombinedStyleWalker : CSharpSyntaxWalker
{
    private readonly IStyleRuleHandler[] _handlers;
    private readonly LintConfiguration _config;

    public CombinedStyleWalker(IStyleRuleHandler[] handlers, LintConfiguration config)
    {
        _handlers = handlers;
        _config = config;
    }

    public List<LintDiagnostic> Diagnostics { get; } = [];

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitClassDeclaration(node, _config, Diagnostics);
        }

        base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitStructDeclaration(node, _config, Diagnostics);
        }

        base.VisitStructDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitInterfaceDeclaration(node, _config, Diagnostics);
        }

        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitEnumDeclaration(node, _config, Diagnostics);
        }

        base.VisitEnumDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitMethodDeclaration(node, _config, Diagnostics);
        }

        base.VisitMethodDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitPropertyDeclaration(node, _config, Diagnostics);
        }

        base.VisitPropertyDeclaration(node);
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitFieldDeclaration(node, _config, Diagnostics);
        }

        base.VisitFieldDeclaration(node);
    }

    public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitEventFieldDeclaration(node, _config, Diagnostics);
        }

        base.VisitEventFieldDeclaration(node);
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitLocalDeclarationStatement(node, _config, Diagnostics);
        }

        base.VisitLocalDeclarationStatement(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitIfStatement(node, _config, Diagnostics);
        }

        base.VisitIfStatement(node);
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitForStatement(node, _config, Diagnostics);
        }

        base.VisitForStatement(node);
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitForEachStatement(node, _config, Diagnostics);
        }

        base.VisitForEachStatement(node);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitWhileStatement(node, _config, Diagnostics);
        }

        base.VisitWhileStatement(node);
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitDoStatement(node, _config, Diagnostics);
        }

        base.VisitDoStatement(node);
    }

    public override void VisitUsingStatement(UsingStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitUsingStatement(node, _config, Diagnostics);
        }

        base.VisitUsingStatement(node);
    }

    public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        foreach (IStyleRuleHandler handler in _handlers)
        {
            handler.VisitConditionalExpression(node, _config, Diagnostics);
        }

        base.VisitConditionalExpression(node);
    }
}
