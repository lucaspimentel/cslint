namespace Cslint.Core.Config;

public interface IConfigProvider
{
    public LintConfiguration GetConfiguration(string filePath);
}
