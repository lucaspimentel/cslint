namespace Cslint.Core.Rules.Tier2;

internal sealed record NamingSymbolGroup(
    HashSet<string> ApplicableKinds,
    HashSet<string> ApplicableAccessibilities,
    HashSet<string> RequiredModifiers);
