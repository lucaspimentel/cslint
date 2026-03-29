using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// Shared enablement logic for CA rules that support both a standard
/// <c>dotnet_diagnostic.CAxxxx.severity</c> key and a legacy custom key.
/// Enabled by default when neither key is present, matching .NET SDK behavior.
/// </summary>
internal static class CaRuleHelper
{
    public static bool IsEnabledByDefault(LintConfiguration configuration, string diagnosticKey, string customKey)
    {
        // Standard diagnostic key takes precedence
        LintSeverity? severity = configuration.GetDiagnosticSeverity(diagnosticKey);

        if (severity is not null)
        {
            return severity is not LintSeverity.None;
        }

        // Legacy custom key (backward compatibility)
        (string? pref, string? _) = configuration.GetValueWithSeverity(customKey);

        if (pref is not null)
        {
            return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Default: enabled (matches .NET SDK behavior for CA rules)
        return true;
    }
}
