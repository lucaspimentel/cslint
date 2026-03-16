using System.Collections.Concurrent;
using Cslint.Core.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Cslint.Core.Engine;

public sealed class DirectoryLinter
{
    private readonly FileLinter _fileLinter;
    private readonly IFileSystem _fileSystem;

    private static readonly HashSet<string> SkippedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".g.cs",
        ".designer.cs",
        ".generated.cs",
    };

    private static readonly HashSet<string> SkippedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "obj",
        "bin",
        "artifacts",
        ".vs",
        ".git",
        "node_modules",
    };

    public DirectoryLinter(FileLinter fileLinter, IFileSystem? fileSystem = null)
    {
        _fileLinter = fileLinter;
        _fileSystem = fileSystem ?? new DefaultFileSystem();
    }

    public async Task<IReadOnlyList<LintDiagnostic>> LintDirectoryAsync(
        string directoryPath,
        IReadOnlyList<string>? excludeGlobs = null,
        CancellationToken cancellationToken = default)
    {
        string fullPath = Path.GetFullPath(directoryPath);
        List<string> files = EnumerateFiles(fullPath, excludeGlobs, _fileSystem).ToList();

        // When semantic mode is enabled, create a single shared compilation for all files
        if (_fileLinter.EnableSemantic)
        {
            var trees = new List<SyntaxTree>(files.Count);

            foreach (string file in files)
            {
                string source = File.ReadAllText(file);
                SourceText sourceText = SourceText.From(source);
                trees.Add(CSharpSyntaxTree.ParseText(sourceText, path: file));
            }

            _fileLinter.Compilation = CompilationFactory.CreateCompilation(trees);
        }

        var fileDiagnostics = new ConcurrentQueue<IReadOnlyList<LintDiagnostic>>();

        await Parallel.ForEachAsync(
            files,
            cancellationToken,
            (file, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    IReadOnlyList<LintDiagnostic> diagnostics = _fileLinter.LintFile(file);

                    if (diagnostics.Count > 0)
                    {
                        fileDiagnostics.Enqueue(diagnostics);
                    }
                }
                catch (Exception ex)
                {
                    fileDiagnostics.Enqueue(
                    [
                        new LintDiagnostic
                        {
                            RuleId = "CSLINT000",
                            Message = $"Error linting file: {ex.Message}",
                            Severity = LintSeverity.Error,
                            FilePath = file,
                            Line = 0,
                            Column = 0,
                        },
                    ]);
                }

                return ValueTask.CompletedTask;
            });

        return fileDiagnostics
            .SelectMany(d => d)
            .OrderBy(d => d.FilePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Line)
            .ThenBy(d => d.Column)
            .ToList();
    }

    private static IEnumerable<string> EnumerateFiles(string directoryPath, IReadOnlyList<string>? excludeGlobs, IFileSystem fileSystem)
    {
        Matcher? excludeMatcher = null;

        if (excludeGlobs is { Count: > 0 })
        {
            excludeMatcher = new Matcher();
            excludeMatcher.AddIncludePatterns(excludeGlobs.Select(p => p.Replace('\\', '/')));
        }

        return fileSystem.EnumerateFiles(directoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
            {
                // Skip generated files
                string fileName = Path.GetFileName(file);

                foreach (string ext in SkippedExtensions)
                {
                    if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                // Skip excluded directories
                string? dir = Path.GetDirectoryName(file);

                while (dir is not null && dir.Length >= directoryPath.Length)
                {
                    string dirName = Path.GetFileName(dir);

                    if (SkippedDirectories.Contains(dirName))
                    {
                        return false;
                    }

                    dir = Path.GetDirectoryName(dir);
                }

                // Apply exclude globs against the relative path
                if (excludeMatcher is not null)
                {
                    string relativePath = Path.GetRelativePath(directoryPath, file).Replace('\\', '/');

                    if (excludeMatcher.Match(relativePath).HasMatches)
                    {
                        return false;
                    }
                }

                return true;
            });
    }
}
