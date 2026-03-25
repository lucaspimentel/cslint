using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class InferredMemberNameRule : IRuleDefinition
{
    private const string TupleKey = "dotnet_style_prefer_inferred_tuple_names";

    private const string AnonKey = "dotnet_style_prefer_inferred_anonymous_type_member_names";

    public string RuleId => "IDE0037";

    public string Name => "InferredMemberName";

    public IReadOnlyList<string> ConfigKeys { get; } = [TupleKey, AnonKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static string? GetInferredName(ExpressionSyntax expression) =>
        expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            ConditionalAccessExpressionSyntax conditionalAccess
                when conditionalAccess.WhenNotNull is MemberBindingExpressionSyntax memberBinding
                => memberBinding.Name.Identifier.Text,
            _ => null,
        };

    private static void CheckTupleArgument(
        ArgumentSyntax argument,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (argument.Parent is not TupleExpressionSyntax)
        {
            return;
        }

        if (argument.NameColon is null)
        {
            return;
        }

        string explicitName = argument.NameColon.Name.Identifier.Text;
        string? inferredName = GetInferredName(argument.Expression);

        if (inferredName is null || explicitName != inferredName)
        {
            return;
        }

        FileLinePositionSpan span = argument.NameColon.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "IDE0037",
                Message = $"Tuple element name '{explicitName}' can be inferred",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private static void CheckAnonymousTypeMember(
        AnonymousObjectMemberDeclaratorSyntax memberDecl,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (memberDecl.NameEquals is null)
        {
            return;
        }

        if (memberDecl.Expression is not IdentifierNameSyntax identifier)
        {
            return;
        }

        if (memberDecl.NameEquals.Name.Identifier.Text != identifier.Identifier.Text)
        {
            return;
        }

        FileLinePositionSpan span = memberDecl.NameEquals.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "IDE0037",
                Message = $"Member name '{identifier.Identifier.Text}' can be inferred",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(TupleKey) is not null ||
        configuration.GetValue(AnonKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? tuplePref, string? _) = context.Configuration.GetValueWithSeverity(TupleKey);
        (string? anonPref, string? _) = context.Configuration.GetValueWithSeverity(AnonKey);

        bool checkTuples = string.Equals(tuplePref, "true", StringComparison.OrdinalIgnoreCase);
        bool checkAnon = string.Equals(anonPref, "true", StringComparison.OrdinalIgnoreCase);

        if (!checkTuples && !checkAnon)
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            switch (node)
            {
                case ArgumentSyntax argument when checkTuples:
                    CheckTupleArgument(argument, context.FilePath, diagnostics);
                    break;

                case AnonymousObjectMemberDeclaratorSyntax memberDecl when checkAnon:
                    CheckAnonymousTypeMember(memberDecl, context.FilePath, diagnostics);
                    break;
            }
        }

        return diagnostics;
    }
}
