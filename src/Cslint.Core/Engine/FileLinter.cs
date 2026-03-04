using Cslint.Core.Config;
using Cslint.Core.Rules;
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

    public IReadOnlyList<LintDiagnostic> LintSource(string filePath, string source, LintConfiguration configuration)
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

        foreach (IRuleDefinition rule in _registry.Rules)
        {
            if (rule.IsEnabled(configuration))
            {
                diagnostics.AddRange(rule.Analyze(context));
            }
        }

        return diagnostics;
    }
}
