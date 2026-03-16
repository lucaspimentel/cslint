using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;
using Cslint.Core.Rules.Tier3;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Engine;

public sealed class FileLinter(RuleRegistry registry, IConfigProvider configProvider)
{
    public bool EnableSemantic { get; set; }

    public CSharpCompilation? Compilation { get; set; }

    public IReadOnlyList<LintDiagnostic> LintFile(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        string source = File.ReadAllText(fullPath);
        LintConfiguration config = configProvider.GetConfiguration(fullPath);

        return LintSource(fullPath, source, config);
    }

    public IReadOnlyList<LintDiagnostic> LintSource(
        string filePath,
        string source,
        LintConfiguration configuration)
    {
        SyntaxTree syntaxTree;
        SourceText sourceText;

        // Reuse the tree from the shared compilation if available (avoids double-parsing
        // and ensures the tree instance matches what the compilation knows about)
        if (Compilation is not null)
        {
            syntaxTree = Compilation.SyntaxTrees.First(t =>
                string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
            sourceText = syntaxTree.GetText();
        }
        else
        {
            sourceText = SourceText.From(source);
            syntaxTree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
        }

        var root = (CSharpSyntaxNode)syntaxTree.GetRoot();

        var context = new RuleContext
        {
            FilePath = filePath,
            SourceText = sourceText,
            SyntaxTree = syntaxTree,
            Root = root,
            Configuration = configuration,
        };

        var diagnostics = new List<LintDiagnostic>();
        List<INamingRuleHandler>? namingHandlers = null;
        List<IStyleRuleHandler>? styleHandlers = null;
        List<IDescendantNodeHandler>? descendantHandlers = null;
        List<ISemanticRuleHandler>? semanticHandlers = null;

        foreach (IRuleDefinition rule in registry.Rules)
        {
            if (!rule.IsEnabled(configuration))
            {
                continue;
            }

            // Batch Tier2 naming rules for a single combined walk
            if (rule is INamingRuleHandler namingHandler)
            {
                namingHandlers ??= [];
                namingHandlers.Add(namingHandler);
                continue;
            }

            // Batch Tier3 walker-based rules
            if (rule is IStyleRuleHandler styleHandler)
            {
                styleHandlers ??= [];
                styleHandlers.Add(styleHandler);
                continue;
            }

            // Batch Tier3 DescendantNodes-based rules
            if (rule is IDescendantNodeHandler descendantHandler)
            {
                descendantHandlers ??= [];
                descendantHandlers.Add(descendantHandler);
                continue;
            }

            // Batch Tier4 semantic rules
            if (rule is ISemanticRuleHandler semanticHandler)
            {
                semanticHandlers ??= [];
                semanticHandlers.Add(semanticHandler);
                continue;
            }

            diagnostics.AddRange(rule.Analyze(context));
        }

        // Run all naming rules in a single tree walk
        if (namingHandlers is not null)
        {
            var walker = new CombinedNamingWalker(namingHandlers);
            walker.Visit(root);
            diagnostics.AddRange(walker.Diagnostics);
        }

        // Run all style walker rules in a single tree walk
        if (styleHandlers is not null)
        {
            var walker = new CombinedStyleWalker(styleHandlers, configuration);
            walker.Visit(root);
            diagnostics.AddRange(walker.Diagnostics);
        }

        // Run all DescendantNodes rules in a single enumeration
        if (descendantHandlers is not null)
        {
            foreach (SyntaxNode node in root.DescendantNodes())
            {
                foreach (IDescendantNodeHandler handler in descendantHandlers)
                {
                    handler.VisitNode(node, configuration, filePath, diagnostics);
                }
            }
        }

        // Run Tier4 semantic rules (only when --semantic is enabled)
        if (EnableSemantic && semanticHandlers is not null)
        {
            SemanticModel model = Compilation is not null
                ? CompilationFactory.GetSemanticModel(Compilation, syntaxTree)
                : CompilationFactory.CreateSemanticModel(syntaxTree);

            foreach (ISemanticRuleHandler handler in semanticHandlers)
            {
                handler.Analyze(context, model, diagnostics);
            }
        }

        // Resolve effective severity from .editorconfig and suppress None-severity diagnostics
        ApplySeverityOverrides(diagnostics, registry.Rules, configuration);

        PragmaSuppressionMap suppressions = PragmaSuppressionMap.Build(root);

        if (suppressions.HasSuppressions)
        {
            diagnostics.RemoveAll(d => suppressions.IsSuppressed(d.RuleId, d.Line));
        }

        return diagnostics;
    }

    private static void ApplySeverityOverrides(
        List<LintDiagnostic> diagnostics,
        IReadOnlyList<IRuleDefinition> rules,
        LintConfiguration configuration)
    {
        // Build a map of ruleId -> effective severity
        Dictionary<string, LintSeverity>? severityByRule = null;

        foreach (IRuleDefinition rule in rules)
        {
            LintSeverity? mostSevere = null;

            foreach (string key in rule.ConfigKeys)
            {
                LintSeverity? parsed = configuration.GetSeverityForKey(key);

                if (parsed is not null && (mostSevere is null || parsed > mostSevere))
                {
                    mostSevere = parsed;
                }
            }

            if (mostSevere is not null)
            {
                severityByRule ??= new Dictionary<string, LintSeverity>();
                severityByRule[rule.RuleId] = mostSevere.Value;
            }
        }

        if (severityByRule is null)
        {
            return;
        }

        // Remove suppressed diagnostics and update severity on remaining ones
        diagnostics.RemoveAll(d =>
            severityByRule.TryGetValue(d.RuleId, out LintSeverity severity) &&
            severity == LintSeverity.None);

        foreach (LintDiagnostic diagnostic in diagnostics)
        {
            if (severityByRule.TryGetValue(diagnostic.RuleId, out LintSeverity severity))
            {
                diagnostic.Severity = severity;
            }
        }
    }
}
