using Cslint.Core.Config;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal sealed class CombinedStyleWalker(IReadOnlyList<IStyleRuleHandler> handlers, LintConfiguration config) : CSharpSyntaxWalker
{
    public List<LintDiagnostic> Diagnostics { get; } = [];

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitClassDeclaration(node, config, Diagnostics);
        }

        base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitStructDeclaration(node, config, Diagnostics);
        }

        base.VisitStructDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitInterfaceDeclaration(node, config, Diagnostics);
        }

        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitEnumDeclaration(node, config, Diagnostics);
        }

        base.VisitEnumDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitMethodDeclaration(node, config, Diagnostics);
        }

        base.VisitMethodDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitPropertyDeclaration(node, config, Diagnostics);
        }

        base.VisitPropertyDeclaration(node);
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitFieldDeclaration(node, config, Diagnostics);
        }

        base.VisitFieldDeclaration(node);
    }

    public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitEventFieldDeclaration(node, config, Diagnostics);
        }

        base.VisitEventFieldDeclaration(node);
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitLocalDeclarationStatement(node, config, Diagnostics);
        }

        base.VisitLocalDeclarationStatement(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitIfStatement(node, config, Diagnostics);
        }

        base.VisitIfStatement(node);
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitForStatement(node, config, Diagnostics);
        }

        base.VisitForStatement(node);
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitForEachStatement(node, config, Diagnostics);
        }

        base.VisitForEachStatement(node);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitWhileStatement(node, config, Diagnostics);
        }

        base.VisitWhileStatement(node);
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitDoStatement(node, config, Diagnostics);
        }

        base.VisitDoStatement(node);
    }

    public override void VisitUsingStatement(UsingStatementSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitUsingStatement(node, config, Diagnostics);
        }

        base.VisitUsingStatement(node);
    }

    public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitConditionalExpression(node, config, Diagnostics);
        }

        base.VisitConditionalExpression(node);
    }

    public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitDestructorDeclaration(node, config, Diagnostics);
        }

        base.VisitDestructorDeclaration(node);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitConstructorDeclaration(node, config, Diagnostics);
        }

        base.VisitConstructorDeclaration(node);
    }

    public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitOperatorDeclaration(node, config, Diagnostics);
        }

        base.VisitOperatorDeclaration(node);
    }

    public override void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitConversionOperatorDeclaration(node, config, Diagnostics);
        }

        base.VisitConversionOperatorDeclaration(node);
    }

    public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitIndexerDeclaration(node, config, Diagnostics);
        }

        base.VisitIndexerDeclaration(node);
    }

    public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
    {
        foreach (IStyleRuleHandler handler in handlers)
        {
            handler.VisitAccessorDeclaration(node, config, Diagnostics);
        }

        base.VisitAccessorDeclaration(node);
    }
}
