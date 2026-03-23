# TODO

## Future rule candidates

Full analysis: [docs/rule-mappings.md — Future Candidates](docs/rule-mappings.md#future-candidates)

- [x] **CA1821 — Remove empty finalizers** (Tier 3, syntax-only) — implemented as CSLINT237
- [x] **CA1805 — Do not initialize unnecessarily** (Tier 3, syntax-only, partial coverage) — implemented as CSLINT238
- [x] **CA1852 — Prefer sealed types** (Tier 3, syntax-only) — implemented as CSLINT239

## Semantic rules (Tier 4)

- [x] **CSLINT300 false-positive mitigation** — filter out CS8019 (unused using) diagnostics where the compilation also reports unresolved type errors (CS0246, CS0234) in the same file; that's a strong signal that NuGet/project references are missing, so the "unused" using is probably a false positive — already implemented in UnusedUsingRule
- [x] **Unused local variables** — surface CS0219 from `SemanticModel.GetDiagnostics()`, same pattern as CSLINT300 — implemented as CSLINT301
- [x] **Unreachable code** — surface CS0162 from `SemanticModel.GetDiagnostics()` — implemented as CSLINT302
- [x] **Unnecessary casts** — detect redundant casts where both types are in-source or BCL (symbol resolution reliable) — implemented as CSLINT306
- [x] **Redundant await** — detect `async` methods that just `return await` a single call; `Task`/`ValueTask` are BCL so symbol resolution is reliable — implemented as CSLINT307
- [x] **Unused private members** — fields/methods/properties with `private` access declared but never referenced within the compilation — implemented as CSLINT308
- [x] **Duplicate enum values** — constant value analysis is purely local, no external type resolution needed — implemented as CSLINT303
- [x] **Self-assignment detection** (`x = x`) — symbol equality check, no external type resolution needed — implemented as CSLINT304
- [x] **Empty catch blocks** — catch blocks that swallow exceptions without logging/rethrowing; structural check on catch clause body — implemented as CSLINT305

## Low priority / high cost

- [ ] **CSLINT239 project-wide type hierarchy** — two-pass architecture to reduce false positives on `SealedTypePreferenceRule` by building an in-memory `HashSet<string>` of inherited type names before the lint pass, so base classes aren't flagged
  - Requires a pre-scan pass in `DirectoryLinter` over all syntax trees to collect `BaseListSyntax` identifiers
  - New `IProjectContext` (or similar) threaded through `RuleContext` to rules that need cross-file info
  - Trade-off: simple-name matching only (no semantic model), so name collisions could suppress diagnostics on unrelated types — rare in practice
  - Single-file mode (`FileLinter.LintFile`) would fall back to current behavior (flag everything)
  - Main cost is architectural: breaks the clean file-at-a-time design for marginal gain on an `Info`-severity rule

## StyleCop rule candidates

Reference: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/tree/master/documentation

### Spacing rules (Tier 3, syntax-only)

- [x] **SA1000 — Keyword spacing** (Tier 3) — enforce space after control-flow keywords (`if (`, `for (`, `while (`) and no space after expression keywords (`typeof(`, `sizeof(`). Alias: SA1000 — implemented as CSLINT254
- [x] **SA1001 — Comma spacing** (Tier 3) — no space before comma, single space after comma. Alias: SA1001 — implemented as CSLINT255
- [x] **SA1002 — Semicolon spacing** (Tier 3) — no space before semicolon in `for` statements, space after when followed by next clause. Alias: SA1002 — implemented as CSLINT256
- [x] **SA1003 — Operator spacing** (Tier 3) — binary operators must have single space on both sides, unary operators no space between operator and operand. Alias: SA1003 — implemented as CSLINT257
- [x] **SA1005 — Single-line comment spacing** (Tier 3) — `//` must be followed by a single space before comment text. Alias: SA1005 — implemented as CSLINT258
- [x] **SA1008/SA1009 — Parenthesis spacing** (Tier 3) — no space after opening paren, no space before closing paren. Aliases: SA1008, SA1009 — implemented as CSLINT259
- [x] **SA1012/SA1013 — Brace spacing** (Tier 3) — correct spacing around opening/closing braces. Aliases: SA1012, SA1013 — implemented as CSLINT260
- [x] **SA1024 — Colon spacing** (Tier 3) — colons in base lists, conditional expressions, etc. must have correct spacing. Alias: SA1024 — implemented as CSLINT261
- [x] **SA1025 — No multiple whitespace** (Tier 3) — no consecutive whitespace characters in a row (except indentation). Alias: SA1025 — implemented as CSLINT262

### Formatting rules (Tier 1, text-level)

- [x] **SA1412 — Store files as UTF-8** (Tier 1) — detect non-UTF-8 BOMs (UTF-16 LE/BE, UTF-32) and optionally require/forbid UTF-8 BOM. Just check first 2-4 bytes for BOM markers. Alias: SA1412 — implemented as CSLINT010

### Ordering rules (Tier 3, syntax-only)

- [x] **SA1201 — Element ordering** (Tier 3) — enforce member ordering within types: fields → constructors → properties → indexers → methods → etc. Alias: SA1201 — implemented as CSLINT268
- [x] **SA1202 — Element access ordering** (Tier 3) — enforce access modifier ordering: public → internal → protected internal → protected → private. Alias: SA1202 — implemented as CSLINT267
- [x] **SA1203 — Constants before fields** (Tier 3) — constants must appear before non-constant fields. Alias: SA1203 — implemented as CSLINT265
- [x] **SA1204 — Static before instance** (Tier 3) — static members must appear before instance members. Alias: SA1204 — implemented as CSLINT266
- [x] **SA1214 — Readonly before non-readonly** (Tier 3) — readonly fields must appear before mutable fields. Alias: SA1214 — implemented as CSLINT264
- [x] **SA1212 — Property accessor ordering** (Tier 3) — get accessor must appear before set accessor. Alias: SA1212
- [x] **SA1213 — Event accessor ordering** (Tier 3) — add accessor must appear before remove accessor. Alias: SA1213
- [x] **SA1208/SA1210 — Using directive ordering** (Tier 3) — System usings first, alphabetical ordering. Aliases: SA1208, SA1209, SA1210, SA1211, SA1216, SA1217 — implemented as CSLINT269

### Readability rules (Tier 3, syntax-only)

- [x] **SA1106 — No empty statements** (Tier 3) — flag empty statements (lone semicolons). Alias: SA1106 — implemented as CSLINT240
- [x] **SA1107 — Single statement per line** (Tier 3) — no multiple statements on one line. Alias: SA1107 — implemented as CSLINT241
- [x] **SA1131 — No Yoda conditions** (Tier 3) — constant must not appear on left side of comparison. Alias: SA1131 — implemented as CSLINT242
- [x] **SA1132 — No combined field declarations** (Tier 3) — each field must be declared on its own line. Alias: SA1132 — implemented as CSLINT243
- [x] **SA1133 — No combined attributes** (Tier 3) — each attribute must be in its own attribute list. Alias: SA1133 — implemented as CSLINT244
- [x] **SA1134 — Attributes on own line** (Tier 3) — attributes must not share line with the element declaration. Alias: SA1134 — implemented as CSLINT245
- [x] **SA1136 — Enum values on separate lines** (Tier 3) — each enum member must be on its own line. Alias: SA1136 — implemented as CSLINT246

### Layout rules (Tier 3, syntax-only)

- [x] **SA1505 — No blank line after opening brace** (Tier 3) — opening braces must not be followed by a blank line. Alias: SA1505 — implemented as CSLINT247
- [x] **SA1508 — No blank line before closing brace** (Tier 3) — closing braces must not be preceded by a blank line. Alias: SA1508 — implemented as CSLINT248
- [x] **SA1509 — No blank line before opening brace** (Tier 3) — opening braces must not be preceded by a blank line. Alias: SA1509 — implemented as CSLINT249
- [x] **SA1516 — Elements separated by blank line** (Tier 3) — adjacent elements (methods, properties, etc.) must be separated by a blank line. Alias: SA1516 — implemented as CSLINT250
- [x] **SA1517 — No blank lines at start of file** (Tier 1) — code must not start with blank lines. Alias: SA1517 — implemented as CSLINT009

### Maintainability rules (Tier 3, syntax-only)

- [x] **SA1401 — Fields must be private** (Tier 3) — non-private fields (except const/static readonly) should be flagged. Alias: SA1401 — implemented as CSLINT251
- [x] **SA1402 — Single type per file** (Tier 3) — each file may only contain a single type declaration. Alias: SA1402 — implemented as CSLINT252
- [x] **SA1413 — Trailing commas in multi-line initializers** (Tier 3) — multi-line initializers/enums should use trailing commas. Alias: SA1413 — implemented as CSLINT253

### Naming rules (Tier 2, syntax walker)

- [x] **SA1314 — Type parameter names must begin with T** (Tier 2) — generic type parameter names must start with capital T. Alias: SA1314 — implemented as CSLINT106
- [x] **SA1304/SA1307/SA1311 — Accessible/readonly/static field casing** (Tier 2) — enhance CSLINT104 FieldNamingRule to handle non-private readonly and static readonly fields requiring PascalCase. Aliases: SA1304, SA1307, SA1311 — enhanced in CSLINT104

## CLI improvements

- [x] **Allow passing multiple paths** — accept multiple path arguments on the command line. Changed to `Argument<string[]>` with `ZeroOrMore` arity and updated lint orchestration to iterate over all provided paths.

### Pragma alias additions for already-covered rules

- [x] **Add pragma aliases** for existing rules that already cover StyleCop equivalents: SA1027→CSLINT002, SA1028→CSLINT001, SA1101→CSLINT204, SA1121→CSLINT208, SA1124→CSLINT006, SA1206→CSLINT205, SA1303→CSLINT105, SA1312→CSLINT103, SA1400→CSLINT206, SA1500→CSLINT202, SA1503→CSLINT228, SA1507→CSLINT008, SA1518→CSLINT004

## Standard .editorconfig rule coverage

Goal: support as many standard .editorconfig rules as possible using standard key names and diagnostic ID aliases. See [docs/rule-coverage-analysis.md](docs/rule-coverage-analysis.md) and [docs/standard-editorconfig-rules.md](docs/standard-editorconfig-rules.md) for full details.

### Migrate custom-key rules to accept standard keys (16 rules)

- [x] **CSLINT008 MultipleBlankLines** — also accept `dotnet_style_allow_multiple_blank_lines_experimental` (standard key)
- [x] **CSLINT010 Utf8FileEncoding** — also accept `charset = utf-8` (universal editorconfig key)
- [x] **CSLINT228–233 experimental blank line rules** — also accept the `_experimental` suffix variants of their keys (IDE2001–IDE2006)
- [x] **CSLINT210 NullChecking** — also accept `dotnet_style_null_propagation` (IDE0031), `dotnet_style_coalesce_expression` (IDE0029), `dotnet_style_prefer_is_null_check_over_reference_equality_method` (IDE0041)
- [x] **CSLINT230 BlankLineAfterBlock** — also accept `dotnet_style_allow_statement_immediately_after_block_experimental` (IDE2003)
- [ ] **CSLINT100–106 naming rules** — also accept the standard 3-part `dotnet_naming_rule`/`dotnet_naming_symbols`/`dotnet_naming_style` system (see naming system task below)

### Implement missing Tier 3 style preference rules (~33 rules)

- [ ] **`dotnet_style_readonly_field`** (IDE0044) — add readonly modifier
- [x] **`csharp_style_throw_expression`** (IDE0016) — use throw expression — covered by CSLINT210, added as pragma alias + config key
- [x] **`csharp_style_inlined_variable_declaration`** (IDE0018) — inline variable declaration — implemented as CSLINT272
- [x] **`csharp_style_conditional_delegate_call`** (IDE1005) — use `?.Invoke()` — implemented as CSLINT271
- [x] **`csharp_style_prefer_switch_expression`** (IDE0066) — use switch expression — implemented as CSLINT273
- [x] **`csharp_style_pattern_matching_over_as_with_null_check`** (IDE0019) — pattern matching over `as` with null check — implemented as CSLINT270
- [ ] **`csharp_style_deconstructed_variable_declaration`** (IDE0042) — deconstruct variable declaration
- [x] **`csharp_style_prefer_local_over_anonymous_function`** (IDE0039) — local function over lambda — implemented as CSLINT276
- [x] **`csharp_style_prefer_method_group_conversion`** (IDE0200) — remove unnecessary lambda — implemented as IDE0200
- [x] **`csharp_style_prefer_top_level_statements`** (IDE0210/IDE0211) — top-level statements preference — implemented as IDE0210
- [ ] **`csharp_style_prefer_readonly_struct`** (IDE0250) — struct can be made readonly
- [ ] **`csharp_style_prefer_readonly_struct_member`** (IDE0251) — member can be made readonly
- [ ] **`csharp_prefer_static_local_function`** (IDE0062) — make local function static
- [ ] **`dotnet_style_prefer_auto_properties`** (IDE0032) — use auto property
- [x] **`dotnet_style_prefer_conditional_expression_over_assignment`** (IDE0045) — ternary for assignment — implemented as CSLINT274
- [x] **`dotnet_style_prefer_conditional_expression_over_return`** (IDE0046) — ternary for return — implemented as CSLINT275
- [ ] **`dotnet_style_parentheses_in_arithmetic_binary_operators`** (IDE0047/IDE0048) — parentheses preferences
- [ ] **`dotnet_style_parentheses_in_relational_binary_operators`** (IDE0047/IDE0048)
- [ ] **`dotnet_style_parentheses_in_other_binary_operators`** (IDE0047/IDE0048)
- [ ] **`dotnet_style_parentheses_in_other_operators`** (IDE0047/IDE0048)
- [ ] **`csharp_style_prefer_null_check_over_type_check`** (IDE0150)
- [ ] **`csharp_prefer_static_anonymous_function`** (IDE0320)
- [ ] **`csharp_prefer_system_threading_lock`** (IDE0330)
- [ ] **`csharp_style_prefer_unbound_generic_type_in_nameof`** (IDE0340)
- [ ] **`csharp_style_prefer_implicitly_typed_lambda_expression`** (IDE0350)
- [ ] **`csharp_style_prefer_simple_property_accessors`** (IDE0360)
- [ ] **`dotnet_style_prefer_foreach_explicit_cast_in_source`** (IDE0220)
- [x] **`dotnet_style_namespace_match_folder`** (IDE0130) — implemented as IDE0130
- [ ] **`dotnet_style_prefer_inferred_tuple_names`** (IDE0037)
- [ ] **`dotnet_style_explicit_tuple_names`** (IDE0033)
- [ ] **`csharp_style_unused_value_expression_statement_preference`** (IDE0058)
- [ ] **`csharp_style_unused_value_assignment_preference`** (IDE0059)
- [ ] **`dotnet_code_quality_unused_parameters`** (IDE0060)

### Implement Tier 3 formatting rules — IDE0055 (~39 rules)

- [x] **New-line rules** (7) — CSLINT279 (`csharp_new_line_before_open_brace`), CSLINT280 (`_before_else`), CSLINT281 (`_before_catch`), CSLINT282 (`_before_finally`), CSLINT283 (`_before_members_in_object_initializers`), CSLINT284 (`_before_members_in_anonymous_types`), CSLINT285 (`_between_query_expression_clauses`)
- [ ] **Indentation rules** (6) — `csharp_indent_case_contents`, `_switch_labels`, `_labels`, `_block_contents`, `_braces`, `_case_contents_when_block`
- [x] **Spacing rules** (22) — 10 standard keys accepted by existing CSLINT254-261 rules + 6 new rules: CSLINT286 (cast), CSLINT287 (method decl), CSLINT288 (method call), CSLINT289 (dot), CSLINT290 (square bracket), CSLINT291 (declaration statement)
- [ ] **Wrap/preserve rules** (2) — `csharp_preserve_single_line_statements`, `csharp_preserve_single_line_blocks`
- [x] **Using directive formatting** (2) — `dotnet_sort_system_directives_first` (CSLINT277), `dotnet_separate_import_directive_groups` (CSLINT278)

### Support standard naming convention system (IDE1006)

- [ ] **Implement 3-part naming rule parser** — parse `dotnet_naming_rule.<name>.symbols`, `dotnet_naming_rule.<name>.style`, `dotnet_naming_rule.<name>.severity` linking to `dotnet_naming_symbols.<name>.*` and `dotnet_naming_style.<name>.*` definitions
  - Support all `applicable_kinds`, `applicable_accessibilities`, `required_modifiers`
  - Support all `capitalization` styles (`pascal_case`, `camel_case`, `first_word_upper`, `all_upper`, `all_lower`)
  - Support `required_prefix`, `required_suffix`, `word_separator`
  - Fall back to existing hardcoded naming rules (CSLINT100–106) when no custom naming rules are configured

### Migrate existing rules to standard diagnostic IDs

- [ ] **Migrate 1:1 CSLINT rules to use standard IDE IDs directly** — e.g., CSLINT202→IDE0011, CSLINT205→IDE0036, CSLINT211→IDE0063, CSLINT212→IDE0090, CSLINT213→IDE0034, etc. Keep old CSLINT IDs as pragma aliases for backward compatibility
- [ ] **Split multi-ID CSLINT rules into individual standard-ID rules** — CSLINT200 (IDE0007/IDE0008), CSLINT201 (IDE0021–IDE0027), CSLINT210 (IDE0029/IDE0031/IDE0041) each cover multiple standard diagnostics; split into separate rules so each emits the correct standard ID
