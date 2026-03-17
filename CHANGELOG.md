# Changelog

## [Unreleased]

## [1.2.0] - 2026-03-16

### Added
- Add opt-in semantic analysis mode (`--semantic`) with shared compilation
- Add CSLINT239 — prefer sealed types (CA1852)
- Add CSLINT301 — unused local variable rule
- Add CSLINT302 — unreachable code rule
- Add CSLINT303 — duplicate enum values rule
- Add CSLINT304 — self-assignment detection rule
- Add CSLINT305 — empty catch block detection rule
- Add CSLINT306 — unnecessary cast detection rule
- Add CSLINT307 — redundant await detection rule

### Changed
- Update GitHub Actions to latest versions and pin to SHAs

### Fixed
- Fix native AOT publish by excluding semantic rules
- Suppress CSLINT300 when references are missing

## [1.1.1] - 2026-03-13

### Changed
- Use GetValueWithSeverity for CSLINT237 and CSLINT238
- Enable CSLINT237 and CSLINT238 in .editorconfig

## [1.1.0] - 2026-03-13

### Added
- Add CSLINT237 — empty finalizer rule (CA1821)
- Add CSLINT238 — do not initialize unnecessarily (CA1805)
- Add `--show-config` CLI switch
- Add CI and Release workflow badges to README
- Add Scoop installation instructions to README

### Changed
- Update README description and platform support

### Fixed
- Fix CSLINT006 editorconfig key in rule-mappings.md

## [1.0.0] - 2026-03-09

### Added
- Initial public release with 40+ rules across three tiers
  - Tier 1: Text-level formatting (indentation, line endings, whitespace, regions, file headers)
  - Tier 2: Naming conventions (types, interfaces, members, fields)
  - Tier 3: Style preferences (var usage, expression-bodied members, brace style, namespaces, pattern matching)
- `.editorconfig`-driven configuration
- Pragma suppression support with third-party ID mapping (SA, IDE, CA)
- Output formats: text (MSBuild-style), JSON, SARIF
- Published as .NET tool on NuGet
- `--list-rules` and `--exclude` CLI options
