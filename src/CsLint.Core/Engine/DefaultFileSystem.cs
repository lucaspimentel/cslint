namespace Cslint.Core.Engine;

public sealed class DefaultFileSystem : IFileSystem
{
    public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, SearchOption searchOption) =>
        Directory.EnumerateFiles(directoryPath, searchPattern, searchOption);
}
