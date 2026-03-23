namespace Cslint.Core.Rules.Tier2;

internal sealed record ParsedNamingRule(
    NamingRuleDefinition Rule,
    NamingSymbolGroup Symbols,
    NamingStyleDefinition Style);
