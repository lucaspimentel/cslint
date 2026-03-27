using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0044: Add readonly modifier.
/// Flags private fields that are only assigned in their initializer or declaring type's constructor.
/// Syntax-only approximation: only checks private fields, skips partial types.
/// </summary>
public sealed class ReadonlyFieldPreferenceRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_style_readonly_field";

    public string RuleId => "IDE0044";

    public string Name => "ReadonlyFieldPreference";

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

        foreach (TypeDeclarationSyntax typeDecl in context.Root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            AnalyzeType(typeDecl, context.FilePath, ref diagnostics);
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private static void AnalyzeType(
        TypeDeclarationSyntax typeDecl,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        // Skip partial types — assignments may exist in other files
        if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        // Collect candidate fields: private, non-readonly, non-const, non-volatile
        Dictionary<string, FieldInfo>? candidates = null;

        foreach (MemberDeclarationSyntax member in typeDecl.Members)
        {
            if (member is not FieldDeclarationSyntax field)
            {
                continue;
            }

            SyntaxTokenList modifiers = field.Modifiers;

            if (modifiers.Any(SyntaxKind.ReadOnlyKeyword) ||
                modifiers.Any(SyntaxKind.ConstKeyword) ||
                modifiers.Any(SyntaxKind.VolatileKeyword))
            {
                continue;
            }

            // Only private fields — other access levels can be assigned from other types
            if (!IsPrivate(modifiers))
            {
                continue;
            }

            bool isStatic = modifiers.Any(SyntaxKind.StaticKeyword);

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                string fieldName = variable.Identifier.Text;
                (candidates ??= new(StringComparer.Ordinal))[fieldName] = new FieldInfo(variable, isStatic);
            }
        }

        if (candidates is null || candidates.Count == 0)
        {
            return;
        }

        // Scan all descendant nodes for assignments to candidate fields
        // This includes nested types (which can access parent's private fields)
        foreach (SyntaxNode node in typeDecl.DescendantNodes())
        {
            switch (node)
            {
                case AssignmentExpressionSyntax assignment:
                    CheckAssignmentTarget(assignment.Left, assignment, typeDecl, candidates);
                    break;

                case PrefixUnaryExpressionSyntax prefix
                    when prefix.IsKind(SyntaxKind.PreIncrementExpression) ||
                         prefix.IsKind(SyntaxKind.PreDecrementExpression):
                    CheckAssignmentTarget(prefix.Operand, prefix, typeDecl, candidates);
                    break;

                case PostfixUnaryExpressionSyntax postfix
                    when postfix.IsKind(SyntaxKind.PostIncrementExpression) ||
                         postfix.IsKind(SyntaxKind.PostDecrementExpression):
                    CheckAssignmentTarget(postfix.Operand, postfix, typeDecl, candidates);
                    break;

                case ArgumentSyntax { RefKindKeyword.RawKind: not 0 } arg:
                    // ref or out argument — conservatively remove from candidates
                    string? refFieldName = GetFieldName(arg.Expression);

                    if (refFieldName is not null && candidates.ContainsKey(refFieldName))
                    {
                        candidates.Remove(refFieldName);
                    }

                    break;
            }

            if (candidates.Count == 0)
            {
                return;
            }
        }

        // Remaining candidates can be readonly
        foreach ((string fieldName, FieldInfo info) in candidates)
        {
            FileLinePositionSpan span = info.Declarator.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = "IDE0044",
                    Message = $"Field '{fieldName}' can be made readonly",
                    Severity = LintSeverity.Info,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }

    private static void CheckAssignmentTarget(
        ExpressionSyntax target,
        SyntaxNode assignmentNode,
        TypeDeclarationSyntax typeDecl,
        Dictionary<string, FieldInfo> candidates)
    {
        string? fieldName = GetFieldName(target);

        if (fieldName is null || !candidates.TryGetValue(fieldName, out FieldInfo info))
        {
            return;
        }

        // Check if this assignment is inside a constructor of the declaring type
        if (IsInConstructorOfType(assignmentNode, typeDecl, info.IsStatic))
        {
            return;
        }

        // Assignment outside constructor — field can't be readonly
        candidates.Remove(fieldName);
    }

    private static string? GetFieldName(ExpressionSyntax expression) =>
        expression switch
        {
            // _fieldName
            IdentifierNameSyntax identifier => identifier.Identifier.Text,

            // this._fieldName or obj._fieldName
            // Since we only track private fields, any member access with a matching name
            // within the declaring type body must be accessing our field
            MemberAccessExpressionSyntax { Name: IdentifierNameSyntax name } => name.Identifier.Text,

            _ => null,
        };

    private static bool IsInConstructorOfType(
        SyntaxNode node,
        TypeDeclarationSyntax typeDecl,
        bool isStaticField)
    {
        // Walk ancestors to find the nearest constructor
        SyntaxNode? current = node.Parent;

        while (current is not null && current != typeDecl)
        {
            if (current is ConstructorDeclarationSyntax ctor)
            {
                // Verify the constructor belongs directly to the declaring type (not a nested type)
                if (ctor.Parent != typeDecl)
                {
                    return false;
                }

                // Static fields must be in static constructors, instance fields in instance constructors
                bool isStaticCtor = ctor.Modifiers.Any(SyntaxKind.StaticKeyword);
                return isStaticField == isStaticCtor;
            }

            // If we hit a lambda, local function, or anonymous method inside a constructor,
            // the assignment is deferred — treat as non-constructor context
            if (current is LambdaExpressionSyntax or
                AnonymousMethodExpressionSyntax or
                LocalFunctionStatementSyntax)
            {
                return false;
            }

            // If we hit a nested type, the constructor doesn't belong to our type
            if (current is TypeDeclarationSyntax)
            {
                return false;
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool IsPrivate(SyntaxTokenList modifiers)
    {
        // No access modifier on a field in a class/struct means private by default
        bool hasAccessModifier = false;

        foreach (SyntaxToken modifier in modifiers)
        {
            switch (modifier.Kind())
            {
                case SyntaxKind.PrivateKeyword:
                    return true;
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.ProtectedKeyword:
                case SyntaxKind.InternalKeyword:
                    hasAccessModifier = true;
                    break;
            }
        }

        // No access modifier = private by default in C#
        return !hasAccessModifier;
    }

    private readonly record struct FieldInfo(VariableDeclaratorSyntax Declarator, bool IsStatic);
}
