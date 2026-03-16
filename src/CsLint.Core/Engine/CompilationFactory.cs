using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Engine;

internal static class CompilationFactory
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> PlatformReferences = new(LoadPlatformReferences);

    public static CSharpCompilation CreateCompilation(IEnumerable<SyntaxTree> trees) =>
        CSharpCompilation.Create(
            "CsLintAnalysis",
            trees,
            PlatformReferences.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    public static SemanticModel GetSemanticModel(CSharpCompilation compilation, SyntaxTree tree) =>
        compilation.GetSemanticModel(tree);

    public static SemanticModel CreateSemanticModel(SyntaxTree tree) =>
        GetSemanticModel(CreateCompilation([tree]), tree);

    private static IReadOnlyList<MetadataReference> LoadPlatformReferences()
    {
        var references = new List<MetadataReference>();

        string? assemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;

        if (assemblies is not null)
        {
            foreach (string path in assemblies.Split(Path.PathSeparator))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        return references;
    }
}
