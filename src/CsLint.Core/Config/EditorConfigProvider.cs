using System.Collections.Concurrent;
using EditorConfig.Core;

namespace Cslint.Core.Config;

public sealed class EditorConfigProvider : IConfigProvider
{
    private readonly ConcurrentDictionary<string, LintConfiguration> _cache = new(StringComparer.OrdinalIgnoreCase);

    private readonly EditorConfigParser _parser = new();

    public LintConfiguration GetConfiguration(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        string? directory = Path.GetDirectoryName(fullPath);

        if (directory is null)
        {
            return LintConfiguration.Empty;
        }

        return _cache.GetOrAdd(directory, _ => ParseConfiguration(fullPath));
    }

    private LintConfiguration ParseConfiguration(string representativeFilePath)
    {
        FileConfiguration config = _parser.Parse(representativeFilePath);
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> kvp in config.Properties)
        {
            properties[kvp.Key] = kvp.Value;
        }

        return new LintConfiguration(properties);
    }
}
