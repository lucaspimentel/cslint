# Changelog

## [Unreleased]

### Added
- Add 31 new CA code quality rules (enabled by default, matching .NET SDK behavior):
  - Bug detection: CA2200 (rethrow), CA2011 (self-assign in setter), CA2219 (throw in finally), CA2014 (stackalloc in loop)
  - Modifier checks: CA1012 (abstract public ctor), CA1047 (protected in sealed), CA1052 (static holder), CA1070 (virtual event)
  - Attribute checks: CA1041 (Obsolete message), CA2019 (ThreadStatic initializer), CA2259 (ThreadStatic non-static)
  - Declaration structure: CA1034 (nested type), CA1040 (empty interface), CA1044 (write-only property), CA1050 (type outside namespace), CA1051 (visible field), CA2211 (visible static field)
  - Enum: CA1028 (enum base type), CA1712 (enum prefix), CA2217 (Flags values)
  - Performance: CA1825 (zero-length array), CA1861 (constant array arg), CA2253 (numeric placeholder)
  - Naming: CA1707 (underscores), CA1714 (Flags plural), CA1716 (keyword match), CA1720 (type name param), CA1721 (property vs Get method), CA1727 (PascalCase placeholder)
  - Other: CA1021 (out params), CA1031 (catch Exception), CA2244 (duplicate index init), CA2245 (self-assign property)
- Add IDE0052 — remove unread private member (assigned but never read), split from former CSLINT308
- Add CA1715 pragma alias mapping to existing CSLINT101 (interface I prefix) and CSLINT106 (type param T prefix)
- Add IDE0022 — prefer block body for methods (paired with IDE0021, same `csharp_style_expression_bodied_methods` config key)
- Add IDE0023 — expression-bodied conversion operators (`csharp_style_expression_bodied_operators`)
- Add IDE0024 — expression-bodied operators (`csharp_style_expression_bodied_operators`)
- Add IDE0026 — expression-bodied indexers (`csharp_style_expression_bodied_indexers`)
- Add IDE0027 — expression-bodied accessors (`csharp_style_expression_bodied_accessors`)
- Add IDE0031 — null propagation (`dotnet_style_null_propagation`), flags ternary null checks like `x != null ? x.Prop : null`
- Add IDE0041 — prefer is null (`dotnet_style_prefer_is_null_check_over_reference_equality_method`), flags `ReferenceEquals(x, null)`

### Changed
- **BREAKING:** Tier 4 semantic rules are now disabled by default when `.editorconfig` key is absent, matching .NET SDK behavior. To enable, add `dotnet_diagnostic.<ID>.severity = warning` to your `.editorconfig`.
- **BREAKING:** Migrate 27 rules with 1:1 standard mappings from CSLINT* to standard IDE* diagnostic IDs (e.g., CSLINT202→IDE0011, CSLINT205→IDE0036, CSLINT207→IDE0065). Old CSLINT IDs are preserved as pragma aliases for backward compatibility. The `--rules` CLI option also resolves old IDs transparently.
- **BREAKING:** Split multi-ID rules into individual standard-ID rules: CSLINT200→IDE0007+IDE0008 (var/explicit type), CSLINT201→IDE0021+IDE0025 (expression-bodied methods/properties), CSLINT210→IDE0029+IDE0016 (null coalescing/throw expression)
- **BREAKING:** Migrate remaining 1:1 rules to standard IDs: CSLINT239→CA1852, CSLINT300→IDE0005, CSLINT306→IDE0004, CSLINT308→IDE0051
- Merge CSLINT234 (InferredMemberNameRule) into IDE0037 — single rule now covers both `dotnet_style_prefer_inferred_tuple_names` and `dotnet_style_prefer_inferred_anonymous_type_member_names`
- Force-disable CA1852 (SealedTypePreferenceRule) to prevent false positives until project-wide type hierarchy support is implemented

## [1.7.0] - 2026-03-25

### Added
- Add IDE0033 — explicit tuple names rule (`dotnet_style_explicit_tuple_names`), flags `tuple.Item1` when named elements are available
- Add IDE0037 — inferred tuple names rule (`dotnet_style_prefer_inferred_tuple_names`), flags redundant explicit tuple element names that match the expression
- Add IDE0340 — unbound generic in nameof rule (`csharp_style_prefer_unbound_generic_type_in_nameof`), flags `nameof(List<int>)` → `nameof(List<>)`
- Add IDE0062 — prefer static local function rule (`csharp_prefer_static_local_function`), flags non-static local functions that don't capture enclosing state
- Add IDE0320 — prefer static anonymous function rule (`csharp_prefer_static_anonymous_function`), flags non-static lambdas and anonymous methods that don't capture enclosing state
- Add IDE0150 — prefer null check over type check rule (`csharp_style_prefer_null_check_over_type_check`), flags `is object` → `is not null` and `is not object` → `is null`
- Add IDE0330 — prefer System.Threading.Lock rule (`csharp_prefer_system_threading_lock`), flags `lock` on `object` fields, `this`, or `typeof`
- Add IDE0350 — prefer implicitly typed lambda rule (`csharp_style_prefer_implicitly_typed_lambda_expression`), flags lambdas with explicit parameter types when implicit typing is preferred
- Add IDE0360 — prefer simple property accessors rule (`csharp_style_prefer_simple_property_accessors`), flags properties with trivial get/set that can be auto-properties

## [1.6.0] - 2026-03-23

### Added
- Add 8 expression/style rules: CSLINT270–276, IDE0200 (pattern matching over `as`, conditional delegate call, inlined variable declaration, switch expression, conditional expression over assignment/return, local over anonymous function, method group conversion)
- Add 2 using-directive rules: CSLINT277–278 (sort System first, separate import groups)
- Add 7 new-line formatting rules: CSLINT279–285 (brace placement, else/catch/finally, initializers, anonymous types, query clauses)
- Add 6 spacing rules: CSLINT286–291 (cast, method parens, dot, square bracket, declaration spacing)
- Add 2 formatting rules: CSLINT292 (indentation), CSLINT293 (preserve single-line)
- Add IDE0210 (top-level statements), IDE0130 (namespace match folder)
- Add IDE1006 — standard 3-part naming convention system (`dotnet_naming_rule`, `dotnet_naming_symbols`, `dotnet_naming_style`)
- Accept standard `.editorconfig` keys as aliases for existing rules: `_experimental` suffix keys for blank line rules (CSLINT008, CSLINT228–233), `charset` for CSLINT010, null-checking keys for CSLINT210, and standard spacing keys for CSLINT254–261

### Changed
- Disable CSLINT100–106 hardcoded naming rules when standard 3-part naming config is present (IDE1006 takes over)
- New rules with 1:1 standard mappings now use the standard IDE diagnostic ID directly (e.g., `IDE0200`) instead of `CSLINT*` IDs
- Use ASCII hyphens instead of Unicode box-drawing characters in `--summary` table separators

### Fixed
- Fix IDE0019 pragma alias (was incorrectly mapped to CSLINT209, now CSLINT270)
- Fix IDE0066 pragma alias (was incorrectly mapped to CSLINT209, now CSLINT273)

## [1.5.0] - 2026-03-22

### Added
- Add `--rules` CLI option to run specific rules ignoring `.editorconfig` (e.g., `--rules CSLINT266,CSLINT268` or `--rules all`)
- Add `--summary` CLI option to show diagnostics grouped by rule ID

## [1.4.1] - 2026-03-22

### Fixed
- Fix CSLINT238 false positives on `const` fields (constants require an initializer, so `const int X = 0` is not unnecessary)
- Fix CSLINT251 false positives on struct fields (public fields are common and accepted in structs)

## [1.4.0] - 2026-03-22

### Added
- Accept multiple path arguments on the command line (e.g., `cslint src/ProjectA src/ProjectB`)

## [1.3.1] - 2026-03-20

### Fixed
- Fix CSLINT106 false positives on type parameters with digit suffixes (e.g., `T0`, `T1`, `T2`)

## [1.3.0] - 2026-03-20

### Added
- Add CSLINT263 — accessor ordering: get before set/init in properties, add before remove in events (SA1212, SA1213)
- Add CSLINT264 — readonly fields must appear before mutable fields (SA1214)
- Add CSLINT265 — constant fields must appear before non-constant fields (SA1203)
- Add CSLINT266 — static members must appear before instance members of the same kind (SA1204)
- Add CSLINT267 — element access modifier ordering: public → internal → protected → private (SA1202)
- Add CSLINT268 — element kind ordering: fields → constructors → properties → methods → nested types (SA1201)
- Add CSLINT269 — using directive ordering: System first, alphabetical, regular → static → alias (SA1208, SA1209, SA1210, SA1211, SA1216, SA1217)
- Add CSLINT010 — store files as UTF-8 encoding check (SA1412), with pragma alias SA1412
- Add 7 StyleCop readability rules: CSLINT240 (no empty statements), CSLINT241 (single statement per line), CSLINT242 (no Yoda conditions), CSLINT243 (no combined field declarations), CSLINT244 (no combined attributes), CSLINT245 (attributes on own line), CSLINT246 (enum values on separate lines)
- Add 5 StyleCop layout rules: CSLINT009 (no blank lines at start of file), CSLINT247 (no blank line after opening brace), CSLINT248 (no blank line before closing brace), CSLINT249 (no blank line before opening brace), CSLINT250 (elements separated by blank line)
- Add 3 StyleCop maintainability rules: CSLINT251 (fields must be private), CSLINT252 (single type per file), CSLINT253 (trailing commas in multi-line initializers)
- Add CSLINT106 — type parameter names must begin with T (SA1314)
- Add 9 StyleCop spacing rules: CSLINT254 (keyword spacing), CSLINT255 (comma spacing), CSLINT256 (semicolon spacing), CSLINT257 (operator spacing), CSLINT258 (comment spacing), CSLINT259 (parenthesis spacing), CSLINT260 (brace spacing), CSLINT261 (colon spacing), CSLINT262 (no multiple whitespace)
- Add 13 StyleCop pragma aliases for already-covered rules (SA1027, SA1028, SA1101, SA1121, SA1124, SA1206, SA1303, SA1312, SA1400, SA1500, SA1503, SA1507, SA1518)
- Add 22 pragma aliases for new StyleCop rules (SA1000–SA1136, SA1401–SA1517)
- Add 12 pragma aliases for ordering rules (SA1201–SA1204, SA1208–SA1211, SA1214, SA1216, SA1217)
- Add CSLINT308 — unused private member detection (fields, methods, properties, events) with pragma aliases IDE0051, IDE0052, CS0169, CS0414

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
