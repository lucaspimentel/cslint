using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0042: Deconstruct variable declaration.
/// Flags local variable declarations with explicit tuple types whose elements are accessed individually.
/// Syntax-only: only detects explicit TupleTypeSyntax declarations, not inferred tuple types.
/// </summary>
public sealed class DeconstructedVariableRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_style_deconstructed_variable_declaration";

    public string RuleId => "IDE0042";

    public string Name => "DeconstructedVariable";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            if (node is LocalDeclarationStatementSyntax localDecl)
            {
                CheckLocalDeclaration(localDecl, context.FilePath, ref diagnostics);
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private static void CheckLocalDeclaration(
        LocalDeclarationStatementSyntax localDecl,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        // Must be a single variable declaration
        if (localDecl.Declaration.Variables.Count != 1)
        {
            return;
        }

        // Type must be an explicit tuple type with named elements
        if (localDecl.Declaration.Type is not TupleTypeSyntax tupleType)
        {
            return;
        }

        // Collect named element names
        HashSet<string>? namedElements = null;

        foreach (TupleElementSyntax element in tupleType.Elements)
        {
            if (element.Identifier.Text is { Length: > 0 } name)
            {
                (namedElements ??= new(StringComparer.Ordinal)).Add(name);
            }
        }

        // Must have at least one named element
        if (namedElements is null)
        {
            return;
        }

        VariableDeclaratorSyntax variable = localDecl.Declaration.Variables[0];
        string varName = variable.Identifier.Text;

        // Must be inside a block (to scan subsequent code)
        if (localDecl.Parent is not BlockSyntax block)
        {
            return;
        }

        int declIndex = block.Statements.IndexOf(localDecl);

        if (declIndex < 0)
        {
            return;
        }

        // Check if the variable is reassigned later — if so, deconstruction might not work
        // Also check if any element is accessed via member access
        bool hasElementAccess = false;

        for (int i = declIndex + 1; i < block.Statements.Count; i++)
        {
            StatementSyntax stmt = block.Statements[i];

            foreach (SyntaxNode descendant in stmt.DescendantNodes())
            {
                if (descendant is AssignmentExpressionSyntax assignment &&
                    assignment.Left is IdentifierNameSyntax assignTarget &&
                    assignTarget.Identifier.Text == varName)
                {
                    // Variable is reassigned — can't safely deconstruct
                    return;
                }

                if (descendant is MemberAccessExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax id,
                        Name: IdentifierNameSyntax memberName,
                    } &&
                    id.Identifier.Text == varName &&
                    namedElements.Contains(memberName.Identifier.Text))
                {
                    hasElementAccess = true;
                }
            }
        }

        if (!hasElementAccess)
        {
            return;
        }

        FileLinePositionSpan span = localDecl.GetLocation().GetLineSpan();

        (diagnostics ??= []).Add(
            new LintDiagnostic
            {
                RuleId = "IDE0042",
                Message = $"Variable '{varName}' can be deconstructed",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
