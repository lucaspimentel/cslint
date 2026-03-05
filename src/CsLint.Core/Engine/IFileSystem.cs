namespace Cslint.Core.Engine;

public interface IFileSystem
{
    IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, SearchOption searchOption);
}
