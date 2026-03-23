namespace Cslint.Core.Rules.Tier2;

internal sealed record NamingStyleDefinition(
    string Capitalization,
    string? RequiredPrefix,
    string? RequiredSuffix,
    string? WordSeparator);
