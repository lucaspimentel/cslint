using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal interface IStyleRuleHandler
{
    void VisitClassDeclaration(ClassDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitStructDeclaration(StructDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitEnumDeclaration(EnumDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitMethodDeclaration(MethodDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitPropertyDeclaration(PropertyDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitIfStatement(IfStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitForStatement(ForStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitForEachStatement(ForEachStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitWhileStatement(WhileStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitDoStatement(DoStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitUsingStatement(UsingStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitConditionalExpression(ConditionalExpressionSyntax node, List<LintDiagnostic> diagnostics) { }
}
