using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class UsingDirectiveOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_using_directive_ordering";

    public string RuleId => "CSLINT269";

    public string Name => "UsingDirectiveOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static string GetUsingName(UsingDirectiveSyntax u) =>
        u.NamespaceOrType?.ToString() ?? u.Name?.ToString() ?? string.Empty;

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        // Collect all groups of using directives (top-level and within namespaces)
        CollectUsings(context.Root, ref diagnostics);

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void CollectUsings(CSharpSyntaxNode root, ref List<LintDiagnostic>? diagnostics)
    {
        // Top-level usings
        if (root is CompilationUnitSyntax compilationUnit && compilationUnit.Usings.Count > 0)
        {
            CheckUsingGroup(compilationUnit.Usings, ref diagnostics);
        }

        // Usings inside namespace declarations
        foreach (BaseNamespaceDeclarationSyntax ns in root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
        {
            if (ns.Usings.Count > 0)
            {
                CheckUsingGroup(ns.Usings, ref diagnostics);
            }
        }
    }

    private void CheckUsingGroup(SyntaxList<UsingDirectiveSyntax> usings, ref List<LintDiagnostic>? diagnostics)
    {
        // Classify usings into three groups: regular, static, alias
        var regular = new List<UsingDirectiveSyntax>();
        var staticUsings = new List<UsingDirectiveSyntax>();
        var aliasUsings = new List<UsingDirectiveSyntax>();

        foreach (UsingDirectiveSyntax u in usings)
        {
            if (u.Alias is not null)
            {
                aliasUsings.Add(u);
            }
            else if (u.StaticKeyword != default)
            {
                staticUsings.Add(u);
            }
            else
            {
                regular.Add(u);
            }
        }

        // Check group ordering: regular -> static -> alias
        // Find the last regular, first static, first alias positions
        CheckGroupOrdering(regular, staticUsings, aliasUsings, ref diagnostics);

        // Check System-first within regular usings
        CheckSystemFirst(regular, ref diagnostics);

        // Check alphabetical ordering within regular usings (within System and non-System groups)
        CheckAlphabeticalOrder(regular, ref diagnostics);

        // Check alphabetical ordering within static usings
        CheckStaticAlphabeticalOrder(staticUsings, ref diagnostics);
    }

    private void CheckGroupOrdering(
        List<UsingDirectiveSyntax> regular,
        List<UsingDirectiveSyntax> staticUsings,
        List<UsingDirectiveSyntax> aliasUsings,
        ref List<LintDiagnostic>? diagnostics)
    {
        int lastRegularLine = regular.Count > 0
            ? regular[^1].GetLocation().GetLineSpan().StartLinePosition.Line
            : -1;

        int firstStaticLine = staticUsings.Count > 0
            ? staticUsings[0].GetLocation().GetLineSpan().StartLinePosition.Line
            : int.MaxValue;

        int lastStaticLine = staticUsings.Count > 0
            ? staticUsings[^1].GetLocation().GetLineSpan().StartLinePosition.Line
            : -1;

        int firstAliasLine = aliasUsings.Count > 0
            ? aliasUsings[0].GetLocation().GetLineSpan().StartLinePosition.Line
            : int.MaxValue;

        // Static usings must come after regular usings
        if (staticUsings.Count > 0 && regular.Count > 0 && firstStaticLine < lastRegularLine)
        {
            foreach (UsingDirectiveSyntax s in staticUsings)
            {
                int line = s.GetLocation().GetLineSpan().StartLinePosition.Line;

                if (line < lastRegularLine)
                {
                    FileLinePositionSpan span = s.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Static using directives must appear after regular using directives",
                            Severity = LintSeverity.Warning,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }
            }
        }

        // Alias usings must come after static usings (and regular usings)
        if (aliasUsings.Count > 0)
        {
            int lastNonAliasLine = Math.Max(lastRegularLine, lastStaticLine);

            if (firstAliasLine < lastNonAliasLine)
            {
                foreach (UsingDirectiveSyntax a in aliasUsings)
                {
                    int line = a.GetLocation().GetLineSpan().StartLinePosition.Line;

                    if (line < lastNonAliasLine)
                    {
                        FileLinePositionSpan span = a.GetLocation().GetLineSpan();

                        (diagnostics ??= []).Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message = "Alias using directives must appear after all other using directives",
                                Severity = LintSeverity.Warning,
                                FilePath = span.Path,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });

                        break;
                    }
                }
            }
        }
    }

    private void CheckSystemFirst(List<UsingDirectiveSyntax> regular, ref List<LintDiagnostic>? diagnostics)
    {
        bool seenNonSystem = false;

        foreach (UsingDirectiveSyntax u in regular)
        {
            string name = GetUsingName(u);
            bool isSystem = name == "System" || name.StartsWith("System.", StringComparison.Ordinal);

            if (isSystem && seenNonSystem)
            {
                FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "System using directives must appear before non-System using directives",
                        Severity = LintSeverity.Warning,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            if (!isSystem)
            {
                seenNonSystem = true;
            }
        }
    }

    private void CheckAlphabeticalOrder(List<UsingDirectiveSyntax> regular, ref List<LintDiagnostic>? diagnostics)
    {
        // Check alphabetical order within System group and within non-System group separately
        string? prevSystem = null;
        string? prevNonSystem = null;

        foreach (UsingDirectiveSyntax u in regular)
        {
            string name = GetUsingName(u);
            bool isSystem = name == "System" || name.StartsWith("System.", StringComparison.Ordinal);

            if (isSystem)
            {
                if (prevSystem is not null && string.Compare(name, prevSystem, StringComparison.Ordinal) < 0)
                {
                    FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = $"Using directives must be sorted alphabetically; '{name}' must come before '{prevSystem}'",
                            Severity = LintSeverity.Warning,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }

                prevSystem = name;
            }
            else
            {
                if (prevNonSystem is not null && string.Compare(name, prevNonSystem, StringComparison.Ordinal) < 0)
                {
                    FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = $"Using directives must be sorted alphabetically; '{name}' must come before '{prevNonSystem}'",
                            Severity = LintSeverity.Warning,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }

                prevNonSystem = name;
            }
        }
    }

    private void CheckStaticAlphabeticalOrder(List<UsingDirectiveSyntax> staticUsings, ref List<LintDiagnostic>? diagnostics)
    {
        string? prev = null;

        foreach (UsingDirectiveSyntax u in staticUsings)
        {
            string name = GetUsingName(u);

            if (prev is not null && string.Compare(name, prev, StringComparison.Ordinal) < 0)
            {
                FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Static using directives must be sorted alphabetically; '{name}' must come before '{prev}'",
                        Severity = LintSeverity.Warning,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            prev = name;
        }
    }
}
