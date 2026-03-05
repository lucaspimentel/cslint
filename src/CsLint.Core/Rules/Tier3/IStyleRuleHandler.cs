using Cslint.Core.Config;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal interface IStyleRuleHandler
{
    void VisitClassDeclaration(ClassDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitStructDeclaration(StructDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitEnumDeclaration(EnumDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitMethodDeclaration(MethodDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitPropertyDeclaration(PropertyDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitFieldDeclaration(FieldDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitIfStatement(IfStatementSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitForStatement(ForStatementSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitForEachStatement(ForEachStatementSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitWhileStatement(WhileStatementSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitDoStatement(DoStatementSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitUsingStatement(UsingStatementSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
    void VisitConditionalExpression(ConditionalExpressionSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) { }
}
