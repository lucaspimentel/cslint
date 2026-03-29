using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class UnusedValueAssignmentRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "IDE0059";

    public string Name => "UnusedValueAssignment";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.IDE0059.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.IDE0059.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            BlockSyntax? body = node switch
            {
                MethodDeclarationSyntax m => m.Body,
                LocalFunctionStatementSyntax lf => lf.Body,
                AccessorDeclarationSyntax a => a.Body,
                ConstructorDeclarationSyntax c => c.Body,
                _ => null,
            };

            if (body is null)
            {
                continue;
            }

            AnalyzeBlock(body, context, model, diagnostics);
        }
    }

    private void AnalyzeBlock(
        BlockSyntax block,
        RuleContext context,
        SemanticModel model,
        List<LintDiagnostic> diagnostics)
    {
        // Track pending writes: symbol -> (location, was-read)
        var pendingWrites = new Dictionary<ISymbol, WriteInfo>(SymbolEqualityComparer.Default);

        // Track how many write sites each local has, so we skip single-assignment locals (CS0219 territory)
        var writeCounts = new Dictionary<ISymbol, int>(SymbolEqualityComparer.Default);

        // First pass: count write sites per local to filter out single-assignment variables
        CountWriteSites(block, model, writeCounts);

        foreach (StatementSyntax statement in block.Statements)
        {
            // For branching/looping statements, conservatively treat all references as reads
            if (IsBranchingStatement(statement))
            {
                MarkAllReferencesAsRead(statement, model, pendingWrites);
                CollectWritesFromBranch(statement, model, pendingWrites, writeCounts);
                continue;
            }

            // For straight-line statements, classify each reference
            ProcessStatement(statement, model, pendingWrites, writeCounts, context, diagnostics);
        }

        // End of block: flag any remaining unconsumed writes (for multi-write locals only)
        ReportUnconsumedWrites(pendingWrites, writeCounts, context, diagnostics);
    }

    private static void CountWriteSites(
        BlockSyntax block,
        SemanticModel model,
        Dictionary<ISymbol, int> writeCounts)
    {
        foreach (SyntaxNode node in block.DescendantNodes())
        {
            switch (node)
            {
                case VariableDeclaratorSyntax { Initializer: not null } declarator:
                {
                    if (model.GetDeclaredSymbol(declarator) is ILocalSymbol local && !IsSkippedVariable(local))
                    {
                        writeCounts[local] = writeCounts.GetValueOrDefault(local) + 1;
                    }

                    break;
                }

                case AssignmentExpressionSyntax { Left: IdentifierNameSyntax id } assignment
                    when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression):
                {
                    if (model.GetSymbolInfo(id).Symbol is ILocalSymbol local && !IsSkippedVariable(local))
                    {
                        writeCounts[local] = writeCounts.GetValueOrDefault(local) + 1;
                    }

                    break;
                }
            }
        }
    }

    private static bool IsSkippedVariable(ILocalSymbol local)
    {
        // Skip discards
        if (local.Name is "_" or "")
        {
            return true;
        }

        // Skip foreach iteration variables
        if (local.IsForEach)
        {
            return true;
        }

        return false;
    }

    private static bool IsBranchingStatement(StatementSyntax statement) =>
        statement is IfStatementSyntax
            or SwitchStatementSyntax
            or ForStatementSyntax
            or ForEachStatementSyntax
            or WhileStatementSyntax
            or DoStatementSyntax
            or TryStatementSyntax
            or LockStatementSyntax
            or UsingStatementSyntax;

    private static void MarkAllReferencesAsRead(
        StatementSyntax statement,
        SemanticModel model,
        Dictionary<ISymbol, WriteInfo> pendingWrites)
    {
        foreach (IdentifierNameSyntax id in statement.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (model.GetSymbolInfo(id).Symbol is ILocalSymbol local &&
                pendingWrites.ContainsKey(local))
            {
                pendingWrites[local] = pendingWrites[local] with { WasRead = true };
            }
        }
    }

    private static void CollectWritesFromBranch(
        StatementSyntax statement,
        SemanticModel model,
        Dictionary<ISymbol, WriteInfo> pendingWrites,
        Dictionary<ISymbol, int> writeCounts)
    {
        // Track writes inside branches so we can continue tracking after the branch
        foreach (SyntaxNode node in statement.DescendantNodes())
        {
            if (node is AssignmentExpressionSyntax { Left: IdentifierNameSyntax id } assignment &&
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                model.GetSymbolInfo(id).Symbol is ILocalSymbol local &&
                writeCounts.ContainsKey(local))
            {
                // After a branch that writes to a variable, mark as read (conservative)
                // and set a new pending write
                pendingWrites[local] = new WriteInfo(assignment.GetLocation(), WasRead: true);
            }
        }
    }

    private void ProcessStatement(
        StatementSyntax statement,
        SemanticModel model,
        Dictionary<ISymbol, WriteInfo> pendingWrites,
        Dictionary<ISymbol, int> writeCounts,
        RuleContext context,
        List<LintDiagnostic> diagnostics)
    {
        switch (statement)
        {
            case LocalDeclarationStatementSyntax localDecl:
                ProcessLocalDeclaration(localDecl, model, pendingWrites, writeCounts, context, diagnostics);
                break;

            case ExpressionStatementSyntax exprStatement:
                ProcessExpressionStatement(exprStatement, model, pendingWrites, writeCounts, context, diagnostics);
                break;

            default:
                // For other straight-line statements (return, throw, etc.), treat all references as reads
                MarkAllReferencesAsRead(statement, model, pendingWrites);
                break;
        }
    }

    private void ProcessLocalDeclaration(
        LocalDeclarationStatementSyntax localDecl,
        SemanticModel model,
        Dictionary<ISymbol, WriteInfo> pendingWrites,
        Dictionary<ISymbol, int> writeCounts,
        RuleContext context,
        List<LintDiagnostic> diagnostics)
    {
        foreach (VariableDeclaratorSyntax declarator in localDecl.Declaration.Variables)
        {
            if (declarator.Initializer is null)
            {
                continue;
            }

            // The initializer expression may read other tracked variables
            MarkReadsInExpression(declarator.Initializer.Value, model, pendingWrites);

            if (model.GetDeclaredSymbol(declarator) is not ILocalSymbol local || IsSkippedVariable(local))
            {
                continue;
            }

            if (!writeCounts.TryGetValue(local, out int count) || count <= 1)
            {
                continue;
            }

            // This is a write — check if there's a pending write for this variable
            if (pendingWrites.TryGetValue(local, out WriteInfo? pending) && !pending.WasRead)
            {
                ReportDiagnostic(pending.Location, local.Name, context, diagnostics);
            }

            pendingWrites[local] = new WriteInfo(declarator.Identifier.GetLocation(), WasRead: false);
        }
    }

    private void ProcessExpressionStatement(
        ExpressionStatementSyntax exprStatement,
        SemanticModel model,
        Dictionary<ISymbol, WriteInfo> pendingWrites,
        Dictionary<ISymbol, int> writeCounts,
        RuleContext context,
        List<LintDiagnostic> diagnostics)
    {
        if (exprStatement.Expression is AssignmentExpressionSyntax
            {
                Left: IdentifierNameSyntax targetId
            } assignment &&
            assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            // The right-hand side may read tracked variables (process reads first)
            MarkReadsInExpression(assignment.Right, model, pendingWrites);

            if (model.GetSymbolInfo(targetId).Symbol is ILocalSymbol local &&
                !IsSkippedVariable(local) &&
                writeCounts.TryGetValue(local, out int count) &&
                count > 1)
            {
                // This is a write — check if there's a pending write for this variable
                if (pendingWrites.TryGetValue(local, out WriteInfo? pending) && !pending.WasRead)
                {
                    ReportDiagnostic(pending.Location, local.Name, context, diagnostics);
                }

                pendingWrites[local] = new WriteInfo(targetId.GetLocation(), WasRead: false);
                return;
            }
        }

        // For non-assignment expressions, treat all identifier references as reads
        MarkAllReferencesAsRead(exprStatement, model, pendingWrites);
    }

    private static void MarkReadsInExpression(
        ExpressionSyntax expression,
        SemanticModel model,
        Dictionary<ISymbol, WriteInfo> pendingWrites)
    {
        foreach (IdentifierNameSyntax id in expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (model.GetSymbolInfo(id).Symbol is ILocalSymbol local &&
                pendingWrites.ContainsKey(local))
            {
                pendingWrites[local] = pendingWrites[local] with { WasRead = true };
            }
        }
    }

    private void ReportUnconsumedWrites(
        Dictionary<ISymbol, WriteInfo> pendingWrites,
        Dictionary<ISymbol, int> writeCounts,
        RuleContext context,
        List<LintDiagnostic> diagnostics)
    {
        foreach ((ISymbol symbol, WriteInfo info) in pendingWrites)
        {
            if (!info.WasRead &&
                writeCounts.TryGetValue(symbol, out int count) &&
                count > 1)
            {
                ReportDiagnostic(info.Location, symbol.Name, context, diagnostics);
            }
        }
    }

    private void ReportDiagnostic(Location location, string variableName, RuleContext context, List<LintDiagnostic> diagnostics)
    {
        LinePosition start = location.GetLineSpan().StartLinePosition;

        diagnostics.Add(new LintDiagnostic
        {
            RuleId = RuleId,
            Message = $"Value assigned to '{variableName}' is never read",
            Severity = DefaultSeverity,
            FilePath = context.FilePath,
            Line = start.Line + 1,
            Column = start.Character + 1,
        });
    }

    private sealed record WriteInfo(Location Location, bool WasRead);
}
