# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is CsLint?

A fast C# linter that reads rules from `.editorconfig`. See [README.md](README.md) for usage and [docs/rule-mappings.md](docs/rule-mappings.md) for the full rule reference.

## Build & Test Commands

```bash
dotnet build              # build all projects
dotnet test               # run all tests
dotnet test --filter "FullyQualifiedName~TrailingWhitespaceRuleTests"  # run a single test class
dotnet test --filter "FullyQualifiedName~TrailingWhitespaceRuleTests.DetectsTrailingSpaces"  # single test method
dotnet run --project src/CsLint.Cli -- [path...]  # lint files or directories (defaults to CWD)
dotnet run --project src/CsLint.Cli -- --list-rules  # list all available rules
dotnet run --project src/CsLint.Cli -- --semantic [path...]  # lint with semantic analysis (Tier 4 rules)
dotnet run --project src/CsLint.Cli -- --rules IDE0011,IDE0036 [path...]  # run only specific rules, ignoring .editorconfig
dotnet run --project src/CsLint.Cli -- --rules all [path...]  # run all rules, ignoring .editorconfig
dotnet run --project src/CsLint.Cli -- --summary [path...]  # show diagnostics grouped by rule
dotnet run --project src/CsLint.Cli -- --show-config [path]  # show resolved .editorconfig settings for a path
```

## Config key conventions by tier

- **Tier 1–3** use `.editorconfig` style keys with `value:severity` format parsed by `GetValueWithSeverity()`:
  - `csharp_prefer_braces = true:warning` — value is a preference, severity is after `:`
  - `IsEnabled` checks `string.Equals(pref, "true", ...)` — rule is opt-in (disabled when key absent)
  - Some rules accept multiple config keys (standard + CsLint aliases) via `GetFirstValue()` — CsLint key takes precedence
- **Tier 4** use `dotnet_diagnostic.<ID>.severity` keys where the raw value *is* the severity:
  - `dotnet_diagnostic.IDE0005.severity = warning` — no colon-separated value
  - `IsEnabled` checks `GetDiagnosticSeverity(...) is not null and not LintSeverity.None` — rule is opt-in (disabled when key absent), matching .NET SDK behavior

## Rule ID convention

New rules that map 1:1 to a standard .NET diagnostic ID must use the standard ID directly as their `RuleId` (e.g., `IDE0200`, not `CSLINT*`). The goal is drop-in replacement for `dotnet format`. Rules with no standard equivalent (SA-origin, CsLint-original) keep `CSLINT*` IDs.

## Key design decisions

- All rules implement `IRuleDefinition` and are manually registered in `RuleRegistry` (no reflection, trim-safe)
- Config comes from `.editorconfig` via `editorconfig` NuGet package, abstracted behind `IConfigProvider`
- `PragmaSuppressionMap` filters diagnostics suppressed by `#pragma warning disable` directives; `PragmaAliasMap` maps third-party IDs (e.g., `SA1313`, `IDE1006`) and legacy CSLINT IDs to canonical rule IDs so existing pragmas still suppress the correct rules
- `FileLinter` orchestrates: parse file → resolve config → run enabled rules → filter pragma suppressions
- `DirectoryLinter` processes files in parallel via `Parallel.ForEachAsync`

## Build settings (Directory.Build.props)

- `net10.0`, `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `UseArtifactsOutput=true`
