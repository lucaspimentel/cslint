using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier2;

internal sealed class CombinedNamingWalker : CSharpSyntaxWalker
{
    private readonly INamingRuleHandler[] _handlers;

    public CombinedNamingWalker(INamingRuleHandler[] handlers)
    {
        _handlers = handlers;
    }

    public List<LintDiagnostic> Diagnostics { get; } = [];

    public override void VisitClassDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitClassDeclaration(node, Diagnostics);
        }

        base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitStructDeclaration(node, Diagnostics);
        }

        base.VisitStructDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitInterfaceDeclaration(node, Diagnostics);
        }

        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitEnumDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitEnumDeclaration(node, Diagnostics);
        }

        base.VisitEnumDeclaration(node);
    }

    public override void VisitRecordDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitRecordDeclaration(node, Diagnostics);
        }

        base.VisitRecordDeclaration(node);
    }

    public override void VisitDelegateDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.DelegateDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitDelegateDeclaration(node, Diagnostics);
        }

        base.VisitDelegateDeclaration(node);
    }

    public override void VisitMethodDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitMethodDeclaration(node, Diagnostics);
        }

        base.VisitMethodDeclaration(node);
    }

    public override void VisitPropertyDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitPropertyDeclaration(node, Diagnostics);
        }

        base.VisitPropertyDeclaration(node);
    }

    public override void VisitEventDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitEventDeclaration(node, Diagnostics);
        }

        base.VisitEventDeclaration(node);
    }

    public override void VisitEventFieldDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitEventFieldDeclaration(node, Diagnostics);
        }

        base.VisitEventFieldDeclaration(node);
    }

    public override void VisitFieldDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitFieldDeclaration(node, Diagnostics);
        }

        base.VisitFieldDeclaration(node);
    }

    public override void VisitParameter(Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitParameter(node, Diagnostics);
        }

        base.VisitParameter(node);
    }

    public override void VisitLocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitLocalDeclarationStatement(node, Diagnostics);
        }

        base.VisitLocalDeclarationStatement(node);
    }

    public override void VisitForEachStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax node)
    {
        foreach (INamingRuleHandler handler in _handlers)
        {
            handler.VisitForEachStatement(node, Diagnostics);
        }

        base.VisitForEachStatement(node);
    }
}
