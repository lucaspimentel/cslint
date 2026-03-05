using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Moq;

namespace Cslint.Core.Tests.Engine;

public class DirectoryLinterTests
{
    private static DirectoryLinter CreateLinter(IFileSystem fileSystem)
    {
        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockConfig = new Mock<IConfigProvider>();
        mockConfig.Setup(p => p.GetConfiguration(It.IsAny<string>()))
            .Returns(LintConfiguration.Empty);

        var fileLinter = new FileLinter(registry, mockConfig.Object);
        return new DirectoryLinter(fileLinter, fileSystem);
    }

    private static Mock<IFileSystem> CreateMockFileSystem(params string[] files)
    {
        var mock = new Mock<IFileSystem>();
        mock.Setup(fs => fs.EnumerateFiles(It.IsAny<string>(), "*.cs", SearchOption.AllDirectories))
            .Returns(files);

        return mock;
    }

    [Fact]
    public async Task LintDirectoryAsync_NoExcludeGlobs_ReturnsAllFiles()
    {
        string root = Path.GetFullPath("/src");
        Mock<IFileSystem> mockFs = CreateMockFileSystem(
            Path.Combine(root, "Foo.cs"),
            Path.Combine(root, "Bar.cs"));

        DirectoryLinter linter = CreateLinter(mockFs.Object);
        await linter.LintDirectoryAsync(root);

        mockFs.Verify(
            fs => fs.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories),
            Times.Once);
    }

    [Theory]
    [InlineData("**/Generated/*.cs")]
    [InlineData("Generated/**")]
    public async Task LintDirectoryAsync_ExcludeGlob_FiltersMatchingFiles(string excludePattern)
    {
        string root = Path.GetFullPath("/src");
        string includedFile = Path.Combine(root, "Foo.cs");
        string excludedFile = Path.Combine(root, "Generated", "Bar.cs");

        Mock<IFileSystem> mockFs = CreateMockFileSystem(includedFile, excludedFile);

        // FileLinter.LintFile reads from disk, so it will throw for fake paths.
        // We only care that excluded files are never passed to FileLinter,
        // so we use a mock FileLinter approach: catch the exception and check which files were attempted.
        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockConfig = new Mock<IConfigProvider>();
        mockConfig.Setup(p => p.GetConfiguration(It.IsAny<string>()))
            .Returns(LintConfiguration.Empty);

        var fileLinter = new FileLinter(registry, mockConfig.Object);
        var directoryLinter = new DirectoryLinter(fileLinter, mockFs.Object);

        IReadOnlyList<LintDiagnostic> diagnostics = await directoryLinter.LintDirectoryAsync(
            root,
            new[] { excludePattern });

        // The excluded file should produce an error diagnostic (file not found),
        // but only for the included file — the excluded one should be filtered out.
        Assert.DoesNotContain(diagnostics, d => d.FilePath == excludedFile && d.RuleId != "CSLINT000");
        // The excluded file should not appear at all (even as CSLINT000 error)
        Assert.DoesNotContain(diagnostics, d => d.FilePath == excludedFile);
    }

    [Fact]
    public async Task LintDirectoryAsync_ExcludeGlob_DoesNotFilterNonMatchingFiles()
    {
        string root = Path.GetFullPath("/src");
        string file = Path.Combine(root, "Foo.cs");

        Mock<IFileSystem> mockFs = CreateMockFileSystem(file);

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockConfig = new Mock<IConfigProvider>();
        mockConfig.Setup(p => p.GetConfiguration(It.IsAny<string>()))
            .Returns(LintConfiguration.Empty);

        var fileLinter = new FileLinter(registry, mockConfig.Object);
        var directoryLinter = new DirectoryLinter(fileLinter, mockFs.Object);

        IReadOnlyList<LintDiagnostic> diagnostics = await directoryLinter.LintDirectoryAsync(
            root,
            new[] { "**/Generated/*.cs" });

        // Foo.cs doesn't match the exclude pattern, so it should be processed.
        // It will produce a CSLINT000 error because the file doesn't exist on disk,
        // which confirms it was NOT excluded.
        Assert.Contains(diagnostics, d => d.FilePath == file);
    }

    [Fact]
    public async Task LintDirectoryAsync_SkipsGeneratedFileExtensions()
    {
        string root = Path.GetFullPath("/src");
        string generatedFile = Path.Combine(root, "Foo.g.cs");
        string designerFile = Path.Combine(root, "Bar.designer.cs");
        string normalFile = Path.Combine(root, "Baz.cs");

        Mock<IFileSystem> mockFs = CreateMockFileSystem(generatedFile, designerFile, normalFile);

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockConfig = new Mock<IConfigProvider>();
        mockConfig.Setup(p => p.GetConfiguration(It.IsAny<string>()))
            .Returns(LintConfiguration.Empty);

        var fileLinter = new FileLinter(registry, mockConfig.Object);
        var directoryLinter = new DirectoryLinter(fileLinter, mockFs.Object);

        IReadOnlyList<LintDiagnostic> diagnostics = await directoryLinter.LintDirectoryAsync(root);

        Assert.DoesNotContain(diagnostics, d => d.FilePath == generatedFile);
        Assert.DoesNotContain(diagnostics, d => d.FilePath == designerFile);
        // normalFile should be attempted (produces CSLINT000 error since it doesn't exist on disk)
        Assert.Contains(diagnostics, d => d.FilePath == normalFile);
    }

    [Fact]
    public async Task LintDirectoryAsync_SkipsExcludedDirectories()
    {
        string root = Path.GetFullPath("/src");
        string binFile = Path.Combine(root, "bin", "Debug", "Foo.cs");
        string objFile = Path.Combine(root, "obj", "Bar.cs");
        string normalFile = Path.Combine(root, "Baz.cs");

        Mock<IFileSystem> mockFs = CreateMockFileSystem(binFile, objFile, normalFile);

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockConfig = new Mock<IConfigProvider>();
        mockConfig.Setup(p => p.GetConfiguration(It.IsAny<string>()))
            .Returns(LintConfiguration.Empty);

        var fileLinter = new FileLinter(registry, mockConfig.Object);
        var directoryLinter = new DirectoryLinter(fileLinter, mockFs.Object);

        IReadOnlyList<LintDiagnostic> diagnostics = await directoryLinter.LintDirectoryAsync(root);

        Assert.DoesNotContain(diagnostics, d => d.FilePath == binFile);
        Assert.DoesNotContain(diagnostics, d => d.FilePath == objFile);
        Assert.Contains(diagnostics, d => d.FilePath == normalFile);
    }
}
