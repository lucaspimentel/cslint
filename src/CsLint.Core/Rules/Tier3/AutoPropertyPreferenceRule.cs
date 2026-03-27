using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0032: Use auto property.
/// Flags properties that are simple wrappers around a private backing field,
/// where the field is not used anywhere else in the type.
/// </summary>
public sealed class AutoPropertyPreferenceRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_style_prefer_auto_properties";

    public string RuleId => "IDE0032";

    public string Name => "AutoPropertyPreference";

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
        if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        // Step 1: Collect private fields (no attributes)
        var fields = new Dictionary<string, FieldDeclarationSyntax>(StringComparer.Ordinal);

        foreach (MemberDeclarationSyntax member in typeDecl.Members)
        {
            if (member is not FieldDeclarationSyntax field)
            {
                continue;
            }

            if (!IsPrivate(field.Modifiers))
            {
                continue;
            }

            // Skip fields with attributes
            if (field.AttributeLists.Count > 0)
            {
                continue;
            }

            // Skip static fields
            if (field.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                fields[variable.Identifier.Text] = field;
            }
        }

        if (fields.Count == 0)
        {
            return;
        }

        // Step 2: Match properties to backing fields
        // Maps fieldName → (property, propertySpan)
        var matches = new Dictionary<string, PropertyDeclarationSyntax>(StringComparer.Ordinal);

        foreach (MemberDeclarationSyntax member in typeDecl.Members)
        {
            if (member is not PropertyDeclarationSyntax property)
            {
                continue;
            }

            // Skip static properties
            if (property.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                continue;
            }

            string? backingField = GetBackingField(property, fields);

            if (backingField is not null && !matches.ContainsKey(backingField))
            {
                matches[backingField] = property;
            }
        }

        if (matches.Count == 0)
        {
            return;
        }

        // Step 3: Check if matched fields are used outside their property
        // Build a set of field spans to exclude (the property bodies)
        var propertySpans = new Dictionary<string, TextSpanInfo>(StringComparer.Ordinal);

        foreach ((string fieldName, PropertyDeclarationSyntax prop) in matches)
        {
            propertySpans[fieldName] = new TextSpanInfo(prop.SpanStart, prop.Span.End);
        }

        // Scan all descendant nodes for field references outside their matched property
        foreach (SyntaxNode node in typeDecl.DescendantNodes())
        {
            string? referencedField = GetFieldReference(node);

            if (referencedField is null || !propertySpans.TryGetValue(referencedField, out TextSpanInfo propSpan))
            {
                continue;
            }

            // Is this reference outside the matched property?
            int nodeStart = node.SpanStart;

            if (nodeStart < propSpan.Start || nodeStart >= propSpan.End)
            {
                // Field is used outside its property — can't be auto-property
                matches.Remove(referencedField);
                propertySpans.Remove(referencedField);

                if (matches.Count == 0)
                {
                    return;
                }
            }
        }

        // Step 4: Report remaining matches
        foreach ((string fieldName, PropertyDeclarationSyntax property) in matches)
        {
            FileLinePositionSpan span = property.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = "IDE0032",
                    Message = $"Property '{property.Identifier.Text}' can be an auto-property",
                    Severity = LintSeverity.Info,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }

    /// <summary>
    /// Checks if a property is a simple wrapper around a single backing field.
    /// Returns the field name if it is, null otherwise.
    /// </summary>
    private static string? GetBackingField(
        PropertyDeclarationSyntax property,
        Dictionary<string, FieldDeclarationSyntax> fields)
    {
        // Expression-bodied property: Type Prop => _field;
        if (property.ExpressionBody is not null)
        {
            return GetSimpleFieldAccess(property.ExpressionBody.Expression, fields);
        }

        if (property.AccessorList is null)
        {
            return null;
        }

        string? getterField = null;
        string? setterField = null;
        bool hasGetter = false;
        bool hasSetter = false;

        foreach (AccessorDeclarationSyntax accessor in property.AccessorList.Accessors)
        {
            if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
            {
                hasGetter = true;
                getterField = GetGetterBackingField(accessor, fields);

                if (getterField is null)
                {
                    return null; // Getter is not a simple field read
                }
            }
            else if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration) ||
                     accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
            {
                hasSetter = true;
                setterField = GetSetterBackingField(accessor, fields);

                if (setterField is null)
                {
                    return null; // Setter is not a simple field write
                }
            }
        }

        if (!hasGetter)
        {
            return null; // Must have a getter
        }

        // If both getter and setter exist, they must reference the same field
        if (hasSetter && !string.Equals(getterField, setterField, StringComparison.Ordinal))
        {
            return null;
        }

        return getterField;
    }

    private static string? GetGetterBackingField(
        AccessorDeclarationSyntax accessor,
        Dictionary<string, FieldDeclarationSyntax> fields)
    {
        // get => _field;
        if (accessor.ExpressionBody is not null)
        {
            return GetSimpleFieldAccess(accessor.ExpressionBody.Expression, fields);
        }

        // get { return _field; }
        if (accessor.Body is { Statements: [ReturnStatementSyntax { Expression: { } returnExpr }] })
        {
            return GetSimpleFieldAccess(returnExpr, fields);
        }

        return null;
    }

    private static string? GetSetterBackingField(
        AccessorDeclarationSyntax accessor,
        Dictionary<string, FieldDeclarationSyntax> fields)
    {
        // set => _field = value;
        if (accessor.ExpressionBody is not null)
        {
            return GetSimpleFieldAssignment(accessor.ExpressionBody.Expression, fields);
        }

        // set { _field = value; }
        if (accessor.Body is { Statements: [ExpressionStatementSyntax { Expression: { } expr }] })
        {
            return GetSimpleFieldAssignment(expr, fields);
        }

        return null;
    }

    /// <summary>
    /// Returns the field name if the expression is a simple field access (_field or this._field).
    /// </summary>
    private static string? GetSimpleFieldAccess(ExpressionSyntax expr, Dictionary<string, FieldDeclarationSyntax> fields)
    {
        string? name = expr switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: IdentifierNameSyntax memberName }
                => memberName.Identifier.Text,
            _ => null,
        };

        return name is not null && fields.ContainsKey(name) ? name : null;
    }

    /// <summary>
    /// Returns the field name if the expression is `_field = value` or `this._field = value`.
    /// </summary>
    private static string? GetSimpleFieldAssignment(ExpressionSyntax expr, Dictionary<string, FieldDeclarationSyntax> fields)
    {
        if (expr is not AssignmentExpressionSyntax
            {
                RawKind: (int)SyntaxKind.SimpleAssignmentExpression,
                Right: IdentifierNameSyntax { Identifier.Text: "value" },
            } assignment)
        {
            return null;
        }

        return GetSimpleFieldAccess(assignment.Left, fields);
    }

    /// <summary>
    /// Returns the field name if the node is a reference to a field (_field or this._field).
    /// </summary>
    private static string? GetFieldReference(SyntaxNode node) =>
        node switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: IdentifierNameSyntax name }
                => name.Identifier.Text,
            _ => null,
        };

    private static bool IsPrivate(SyntaxTokenList modifiers)
    {
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

        return !hasAccessModifier;
    }

    private readonly record struct TextSpanInfo(int Start, int End);
}
