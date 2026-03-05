# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is CsLint?

A fast C# linter that reads rules from `.editorconfig`. Uses Roslyn **syntax-only** parsing (no compilation/semantic model) to achieve sub-100ms single-file linting, as an alternative to `dotnet format --verify-no-changes`.

## Build & Test Commands

```bash
dotnet build              # build all projects
dotnet test               # run all tests
dotnet test --filter "FullyQualifiedName~TrailingWhitespaceRuleTests"  # run a single test class
dotnet test --filter "FullyQualifiedName~TrailingWhitespaceRuleTests.DetectsTrailingSpaces"  # single test method
dotnet run --project src/CsLint.Cli -- <path>  # lint a file or directory
```

## Architecture

Four projects in `CsLint.slnx`:

- **CsLint.Core** — rules engine, config, formatters (class library)
- **CsLint.Cli** — console app entry point using System.CommandLine
- **CsLint.Core.Tests** — xUnit tests with Moq
- **CsLint.Benchmarks** — BenchmarkDotNet performance benchmarks

### Rules are organized in tiers

- **Tier1** (`Rules/Tier1/`) — text-level formatting checks (indentation, line endings, trailing whitespace, final newline, max line length). No syntax tree needed.
- **Tier2** (`Rules/Tier2/`) — naming convention checks using `CSharpSyntaxWalker` (type naming, interface prefix, member naming, field naming, etc.). Shared `NamingHelper` utility.
- **Tier3** (`Rules/Tier3/`) — style preference checks via syntax tree analysis (`var` usage, expression-bodied members, brace style, namespace declarations, etc.).

### Key design decisions

- All rules implement `IRuleDefinition` and are manually registered in `RuleRegistry` (no reflection, trim-safe)
- Config comes from `.editorconfig` via `editorconfig` NuGet package, abstracted behind `IConfigProvider`
- `FileLinter` orchestrates: parse file → resolve config → run enabled rules
- `DirectoryLinter` processes files in parallel via `Parallel.ForEachAsync`
- Exit codes: 0 = clean, 1 = violations, 2 = error
- Output formats: text (MSBuild-style), JSON, SARIF

### Build settings (Directory.Build.props)

- `net10.0`, `LangVersion=latest`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `UseArtifactsOutput=true`
