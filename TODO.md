# TODO

## Low priority / high cost

- [ ] **CSLINT239 project-wide type hierarchy** — two-pass architecture to reduce false positives on `SealedTypePreferenceRule` by building an in-memory `HashSet<string>` of inherited type names before the lint pass, so base classes aren't flagged
  - Requires a pre-scan pass in `DirectoryLinter` over all syntax trees to collect `BaseListSyntax` identifiers
  - New `IProjectContext` (or similar) threaded through `RuleContext` to rules that need cross-file info
  - Trade-off: simple-name matching only (no semantic model), so name collisions could suppress diagnostics on unrelated types — rare in practice
  - Single-file mode (`FileLinter.LintFile`) would fall back to current behavior (flag everything)
  - Main cost is architectural: breaks the clean file-at-a-time design for marginal gain on an `Info`-severity rule

## Match .NET SDK default rule enablement

- [ ] ⚠️ **BREAKING CHANGE (major version bump)** — **Mirror .NET SDK defaults for CA vs IDE rules** — in the .NET SDK, code quality rules (CA*) are enabled by default since .NET 5, while code style rules (IDE*) are disabled by default on command-line builds. CsLint should match this: CA-mapped rules (CSLINT237/CA1821, CSLINT238/CA1805, CSLINT239/CA1852) should be enabled by default without requiring `.editorconfig` opt-in; IDE style rules should continue requiring explicit config
  - Currently all Tier 3 rules use `IsEnabled` that checks for a config key — CA rules need a different default (enabled when key absent)
  - Tier 4 rules already default to enabled (`GetSeverityForKey != None`), so the CA pattern exists
  - Reference: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview

## Standard .editorconfig rule coverage

Goal: support as many standard .editorconfig rules as possible using standard key names and diagnostic ID aliases. See [docs/rule-coverage-analysis.md](docs/rule-coverage-analysis.md) and [docs/standard-editorconfig-rules.md](docs/standard-editorconfig-rules.md) for full details.

### Implement missing Tier 3 style preference rules

- [ ] **`dotnet_style_readonly_field`** (IDE0044) — add readonly modifier
- [ ] **`csharp_style_deconstructed_variable_declaration`** (IDE0042) — deconstruct variable declaration
- [ ] **`csharp_style_prefer_readonly_struct`** (IDE0250) — struct can be made readonly
- [ ] **`csharp_style_prefer_readonly_struct_member`** (IDE0251) — member can be made readonly
- [x] **`csharp_prefer_static_local_function`** (IDE0062) — make local function static
- [ ] **`dotnet_style_prefer_auto_properties`** (IDE0032) — use auto property
- [ ] **`dotnet_style_parentheses_in_arithmetic_binary_operators`** (IDE0047/IDE0048) — parentheses preferences
- [ ] **`dotnet_style_parentheses_in_relational_binary_operators`** (IDE0047/IDE0048)
- [ ] **`dotnet_style_parentheses_in_other_binary_operators`** (IDE0047/IDE0048)
- [ ] **`dotnet_style_parentheses_in_other_operators`** (IDE0047/IDE0048)
- [x] **`csharp_style_prefer_null_check_over_type_check`** (IDE0150)
- [x] **`csharp_prefer_static_anonymous_function`** (IDE0320)
- [ ] **`csharp_prefer_system_threading_lock`** (IDE0330)
- [x] **`csharp_style_prefer_unbound_generic_type_in_nameof`** (IDE0340)
- [ ] **`csharp_style_prefer_implicitly_typed_lambda_expression`** (IDE0350)
- [ ] **`csharp_style_prefer_simple_property_accessors`** (IDE0360)
- [ ] **`dotnet_style_prefer_foreach_explicit_cast_in_source`** (IDE0220)
- [x] **`dotnet_style_prefer_inferred_tuple_names`** (IDE0037)
- [x] **`dotnet_style_explicit_tuple_names`** (IDE0033)
- [ ] **`csharp_style_unused_value_expression_statement_preference`** (IDE0058)
- [ ] **`csharp_style_unused_value_assignment_preference`** (IDE0059)
- [ ] **`dotnet_code_quality_unused_parameters`** (IDE0060)

### Migrate existing rules to standard diagnostic IDs

- [ ] ⚠️ **BREAKING CHANGE (major version bump)** — **Migrate 1:1 CSLINT rules to use standard IDE IDs directly** — e.g., CSLINT202→IDE0011, CSLINT205→IDE0036, CSLINT211→IDE0063, CSLINT212→IDE0090, CSLINT213→IDE0034, etc. Keep old CSLINT IDs as pragma aliases for backward compatibility
- [ ] ⚠️ **BREAKING CHANGE (major version bump)** — **Split multi-ID CSLINT rules into individual standard-ID rules** — CSLINT200 (IDE0007/IDE0008), CSLINT201 (IDE0021–IDE0027), CSLINT210 (IDE0029/IDE0031/IDE0041) each cover multiple standard diagnostics; split into separate rules so each emits the correct standard ID
