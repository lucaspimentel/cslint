# Changelog

## [Unreleased]

### Added
- Add 7 StyleCop readability rules: CSLINT240 (no empty statements), CSLINT241 (single statement per line), CSLINT242 (no Yoda conditions), CSLINT243 (no combined field declarations), CSLINT244 (no combined attributes), CSLINT245 (attributes on own line), CSLINT246 (enum values on separate lines)
- Add 5 StyleCop layout rules: CSLINT009 (no blank lines at start of file), CSLINT247 (no blank line after opening brace), CSLINT248 (no blank line before closing brace), CSLINT249 (no blank line before opening brace), CSLINT250 (elements separated by blank line)
- Add 3 StyleCop maintainability rules: CSLINT251 (fields must be private), CSLINT252 (single type per file), CSLINT253 (trailing commas in multi-line initializers)
- Add CSLINT106 — type parameter names must begin with T (SA1314)
- Add 9 StyleCop spacing rules: CSLINT254 (keyword spacing), CSLINT255 (comma spacing), CSLINT256 (semicolon spacing), CSLINT257 (operator spacing), CSLINT258 (comment spacing), CSLINT259 (parenthesis spacing), CSLINT260 (brace spacing), CSLINT261 (colon spacing), CSLINT262 (no multiple whitespace)
- Add 13 StyleCop pragma aliases for already-covered rules (SA1027, SA1028, SA1101, SA1121, SA1124, SA1206, SA1303, SA1312, SA1400, SA1500, SA1503, SA1507, SA1518)
- Add 22 pragma aliases for new StyleCop rules (SA1000–SA1136, SA1401–SA1517)

### Changed
- Enhance CSLINT104 (FieldNamingRule) to check non-private readonly and static readonly fields for PascalCase (SA1304, SA1307, SA1311)

## [1.2.1] - 2026-03-18

### Added
- Add `--version` CLI option

### Fixed
- Fix CSLINT230 false positive when preprocessor directives appear between a block and the next statement
- Fix test project build when semantic rules are excluded

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
