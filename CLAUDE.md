# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is CsLint?

A fast C# linter that reads rules from `.editorconfig`. Uses Roslyn **syntax-only** parsing as an alternative to `dotnet format --verify-no-changes`, with opt-in semantic analysis (`--semantic`) for deeper checks.

## Build & Test Commands

```bash
dotnet build              # build all projects
dotnet test               # run all tests
dotnet test --filter "FullyQualifiedName~TrailingWhitespaceRuleTests"  # run a single test class
dotnet test --filter "FullyQualifiedName~TrailingWhitespaceRuleTests.DetectsTrailingSpaces"  # single test method
dotnet run --project src/CsLint.Cli -- [path]  # lint a file or directory (defaults to CWD)
dotnet run --project src/CsLint.Cli -- --list-rules  # list all available rules
dotnet run --project src/CsLint.Cli -- --semantic [path]  # lint with semantic analysis (Tier 4 rules)
dotnet run --project src/CsLint.Cli -- --show-config [path]  # show resolved .editorconfig settings for a path
```

## Architecture

Four projects in `CsLint.slnx`:

- **CsLint.Core** — rules engine, config, formatters (class library)
- **CsLint.Cli** — console app entry point using System.CommandLine
- **CsLint.Core.Tests** — xUnit tests with Moq
- **CsLint.Benchmarks** — BenchmarkDotNet performance benchmarks

### Rules are organized in tiers

- **Tier1** (`Rules/Tier1/`) — text-level formatting checks (indentation, line endings, trailing whitespace, final newline, max line length, no `#region` directives, file header, multiple blank lines). No syntax tree needed.
- **Tier2** (`Rules/Tier2/`) — naming convention checks using `CSharpSyntaxWalker` (type naming, interface prefix, member naming, field naming, etc.). Shared `NamingHelper` utility.
- **Tier3** (`Rules/Tier3/`) — style preference checks via syntax tree analysis (`var` usage, expression-bodied members, brace style, namespace declarations, sealed types, empty catch blocks, etc.).
- **Tier4** (`Rules/Tier4/`) — semantic analysis rules requiring Roslyn `SemanticModel` (unused usings, unused locals, unreachable code, duplicate enum values, self-assignment, unnecessary casts, redundant await). Only active with `--semantic` flag.

### Config key conventions by tier

- **Tier 1–3** use `.editorconfig` style keys with `value:severity` format parsed by `GetValueWithSeverity()`:
  - `csharp_prefer_braces = true:warning` — value is a preference, severity is after `:`
  - `IsEnabled` checks `string.Equals(pref, "true", ...)` — rule is opt-in (disabled when key absent)
- **Tier 4** use `dotnet_diagnostic.CSLINT*.severity` keys where the raw value *is* the severity:
  - `dotnet_diagnostic.CSLINT300.severity = warning` — no colon-separated value
  - `IsEnabled` checks `GetSeverityForKey(...) != LintSeverity.None` — rule is enabled by default (active when key absent)

### Key design decisions

- All rules implement `IRuleDefinition` and are manually registered in `RuleRegistry` (no reflection, trim-safe)
- Config comes from `.editorconfig` via `editorconfig` NuGet package, abstracted behind `IConfigProvider`
- `PragmaSuppressionMap` filters diagnostics suppressed by `#pragma warning disable` directives; `PragmaAliasMap` maps third-party IDs (e.g., `SA1313`, `IDE1006`) to CsLint IDs so existing pragmas also suppress corresponding CsLint rules
- `FileLinter` orchestrates: parse file → resolve config → run enabled rules → filter pragma suppressions
- `DirectoryLinter` processes files in parallel via `Parallel.ForEachAsync`
- Exit codes: 0 = clean, 1 = violations, 2 = error
- Output formats: text (MSBuild-style), JSON, SARIF

### Build settings (Directory.Build.props)

- `net10.0`, `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `UseArtifactsOutput=true`
