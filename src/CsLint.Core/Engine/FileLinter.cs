using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;
using Cslint.Core.Rules.Tier3;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Engine;

public sealed class FileLinter
{
    private readonly RuleRegistry _registry;
    private readonly IConfigProvider _configProvider;

    public FileLinter(RuleRegistry registry, IConfigProvider configProvider)
    {
        _registry = registry;
        _configProvider = configProvider;
    }

    public IReadOnlyList<LintDiagnostic> LintFile(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        string source = File.ReadAllText(fullPath);
        LintConfiguration config = _configProvider.GetConfiguration(fullPath);

        return LintSource(fullPath, source, config);
    }

    public IReadOnlyList<LintDiagnostic> LintSource(
        string filePath,
        string source,
        LintConfiguration configuration)
    {
        SourceText sourceText = SourceText.From(source);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
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

        foreach (IRuleDefinition rule in _registry.Rules)
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

            diagnostics.AddRange(rule.Analyze(context));
        }

        // Run all naming rules in a single tree walk
        if (namingHandlers is not null)
        {
            var walker = new CombinedNamingWalker([.. namingHandlers]);
            walker.Visit(root);
            diagnostics.AddRange(walker.Diagnostics);
        }

        // Run all style walker rules in a single tree walk
        if (styleHandlers is not null)
        {
            var walker = new CombinedStyleWalker([.. styleHandlers], configuration);
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

        PragmaSuppressionMap suppressions = PragmaSuppressionMap.Build(root);

        if (suppressions.HasSuppressions)
        {
            diagnostics.RemoveAll(d => suppressions.IsSuppressed(d.RuleId, d.Line));
        }

        return diagnostics;
    }
}
