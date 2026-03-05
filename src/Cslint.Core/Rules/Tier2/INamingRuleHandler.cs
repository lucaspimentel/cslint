using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

internal interface INamingRuleHandler
{
    void VisitClassDeclaration(ClassDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitStructDeclaration(StructDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitEnumDeclaration(EnumDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitRecordDeclaration(RecordDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitDelegateDeclaration(DelegateDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitMethodDeclaration(MethodDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitPropertyDeclaration(PropertyDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitEventDeclaration(EventDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitParameter(ParameterSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node, List<LintDiagnostic> diagnostics) { }
    void VisitForEachStatement(ForEachStatementSyntax node, List<LintDiagnostic> diagnostics) { }
}
