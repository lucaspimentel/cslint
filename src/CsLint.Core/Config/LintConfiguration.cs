namespace Cslint.Core.Config;

public sealed class LintConfiguration
{
    private readonly IReadOnlyDictionary<string, string> _properties;

    public LintConfiguration(IReadOnlyDictionary<string, string> properties)
    {
        _properties = properties;
    }

    public string? GetValue(string key) =>
        _properties.TryGetValue(key, out string? value) ? value : null;

    public bool GetBool(string key, bool defaultValue = false)
    {
        string? value = GetValue(key);

        if (value is null)
        {
            return defaultValue;
        }

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    public int? GetInt(string key)
    {
        string? value = GetValue(key);

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

        int colonIndex = raw.IndexOf(':');

        if (colonIndex < 0)
        {
            return (raw, null);
        }

        return (raw[..colonIndex], raw[(colonIndex + 1)..]);
    }

    public static LintConfiguration Empty { get; } = new(new Dictionary<string, string>());
}
