using System.Collections.Concurrent;
using Cslint.Core.Rules;

namespace Cslint.Core.Config;

public sealed class LintConfiguration(IReadOnlyDictionary<string, string> properties)
{
    private readonly ConcurrentDictionary<string, (string? value, string? severity)> _severityCache = new();

    public static LintConfiguration Empty { get; } = new(new Dictionary<string, string>());

    public IReadOnlyDictionary<string, string> Properties => properties;

    public static LintSeverity? ParseSeverity(string? severity) =>
        severity?.ToLowerInvariant() switch
        {
            "error" => LintSeverity.Error,
            "warning" => LintSeverity.Warning,
            "suggestion" => LintSeverity.Info,
            "silent" or "none" => LintSeverity.None,
            _ => null,
        };

    public string? GetValue(string key) =>
        properties.TryGetValue(key, out string? value) ? value : null;

    public bool GetBool(string key, bool defaultValue = false)
    {
        (string? value, string? _) = GetValueWithSeverity(key);

        if (value is null)
        {
            return defaultValue;
        }

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    public int? GetInt(string key)
    {
        (string? value, string? _) = GetValueWithSeverity(key);

        if (value is null)
        {
            return null;
        }

        return int.TryParse(value, out int result) ? result : null;
    }

    public (string? value, string? severity) GetValueWithSeverity(string key)
    {
        string? raw = GetValue(key);

        if (raw is null)
        {
            return (null, null);
        }

        return _severityCache.GetOrAdd(key, _ =>
        {
            int colonIndex = raw.IndexOf(':');

            if (colonIndex < 0)
            {
                return (raw, null);
            }

            return (raw[..colonIndex], raw[(colonIndex + 1)..]);
        });
    }

    public LintSeverity? GetSeverityForKey(string key)
    {
        (string? _, string? severity) = GetValueWithSeverity(key);
        return ParseSeverity(severity);
    }
}
