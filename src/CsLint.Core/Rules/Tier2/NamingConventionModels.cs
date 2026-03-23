namespace Cslint.Core.Rules.Tier2;

internal sealed record NamingRuleDefinition(
    string Name,
    string SymbolGroupName,
    string StyleName,
    string Severity);
