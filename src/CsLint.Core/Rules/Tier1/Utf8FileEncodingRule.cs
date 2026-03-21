using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class Utf8FileEncodingRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_store_files_as_utf8";

    public string RuleId => "CSLINT010";

    public string Name => "Utf8FileEncoding";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static string? DetectNonUtf8Bom(byte[] prefix)
    {
        // Check UTF-32 LE before UTF-16 LE (shares first 2 bytes)
        if (prefix.Length >= 4 && prefix[0] == 0xFF && prefix[1] == 0xFE && prefix[2] == 0x00 && prefix[3] == 0x00)
        {
            return "UTF-32 LE";
        }

        if (prefix.Length >= 4 && prefix[0] == 0x00 && prefix[1] == 0x00 && prefix[2] == 0xFE && prefix[3] == 0xFF)
        {
            return "UTF-32 BE";
        }

        if (prefix.Length >= 2 && prefix[0] == 0xFF && prefix[1] == 0xFE)
        {
            return "UTF-16 LE";
        }

        if (prefix.Length >= 2 && prefix[0] == 0xFE && prefix[1] == 0xFF)
        {
            return "UTF-16 BE";
        }

        // UTF-8 BOM (EF BB BF) and no BOM are both accepted
        return null;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetBool(ConfigKey);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (context.FilePrefix is not { } prefix)
        {
            return [];
        }

        string? encoding = DetectNonUtf8Bom(prefix);

        if (encoding is null)
        {
            return [];
        }

        return
        [
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"File is encoded as {encoding}; store files as UTF-8",
                Severity = LintSeverity.Warning,
                FilePath = context.FilePath,
                Line = 1,
                Column = 1,
            },
        ];
    }
}
