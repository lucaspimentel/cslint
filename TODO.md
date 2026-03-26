# TODO

## Low priority / high cost

- [ ] **CA1852 project-wide type hierarchy** — two-pass architecture to reduce false positives on `SealedTypePreferenceRule` by building an in-memory `HashSet<string>` of inherited type names before the lint pass, so base classes aren't flagged. **Rule is force-disabled until this is implemented.**
  - Requires a pre-scan pass in `DirectoryLinter` over all syntax trees to collect `BaseListSyntax` identifiers
  - New `IProjectContext` (or similar) threaded through `RuleContext` to rules that need cross-file info
  - Trade-off: simple-name matching only (no semantic model), so name collisions could suppress diagnostics on unrelated types — rare in practice
  - Single-file mode (`FileLinter.LintFile`) would fall back to current behavior (flag everything)
  - Main cost is architectural: breaks the clean file-at-a-time design for marginal gain on an `Info`-severity rule

## Match .NET SDK default rule enablement

- [ ] ⚠️ **BREAKING CHANGE (major version bump)** — **Mirror .NET SDK defaults for CA vs IDE rules** — in the .NET SDK, code quality rules (CA*) are enabled by default since .NET 5, while code style rules (IDE*) are disabled by default on command-line builds. CsLint should match this: CA-mapped rules (CA1821, CA1805, CA1852) should be enabled by default without requiring `.editorconfig` opt-in; IDE style rules should continue requiring explicit config
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
- [x] **`csharp_prefer_system_threading_lock`** (IDE0330)
- [x] **`csharp_style_prefer_unbound_generic_type_in_nameof`** (IDE0340)
- [x] **`csharp_style_prefer_implicitly_typed_lambda_expression`** (IDE0350)
- [x] **`csharp_style_prefer_simple_property_accessors`** (IDE0360)
- [ ] **`dotnet_style_prefer_foreach_explicit_cast_in_source`** (IDE0220)
- [x] **`dotnet_style_prefer_inferred_tuple_names`** (IDE0037)
- [x] **`dotnet_style_explicit_tuple_names`** (IDE0033)
- [ ] **`csharp_style_unused_value_expression_statement_preference`** (IDE0058)
- [ ] **`csharp_style_unused_value_assignment_preference`** (IDE0059)
- [ ] **`dotnet_code_quality_unused_parameters`** (IDE0060)

### Migrate existing rules to standard diagnostic IDs

- [x] ⚠️ **BREAKING CHANGE (major version bump)** — **Migrate 1:1 CSLINT rules to use standard IDE IDs directly** — e.g., CSLINT202→IDE0011, CSLINT205→IDE0036, CSLINT211→IDE0063, CSLINT212→IDE0090, CSLINT213→IDE0034, etc. Keep old CSLINT IDs as pragma aliases for backward compatibility. **Note:** CSLINT234 (IDE0037) excluded — conflicts with InferredTupleNamesRule; needs merge instead.
- [x] ⚠️ **BREAKING CHANGE (major version bump)** — **Split multi-ID CSLINT rules into individual standard-ID rules** — CSLINT200→IDE0007+IDE0008, CSLINT201→IDE0021+IDE0025 (implemented), CSLINT210→IDE0029+IDE0016 (implemented). Unimplemented diagnostics added to backlog below.

### Implement CA rules (syntax-only, no semantic analysis needed)

#### Design (CA10xx)
- [x] **CA1012** — Abstract types should not have public constructors
- [x] **CA1021** — Avoid out parameters on public methods
- [x] **CA1028** — Enum storage should be Int32
- [x] **CA1031** — Do not catch general exception types (detect `catch (Exception)`)
- [x] **CA1034** — Nested types should not be visible
- [x] **CA1040** — Avoid empty interfaces
- [x] **CA1041** — Provide ObsoleteAttribute message
- [x] **CA1044** — Properties should not be write only
- [x] **CA1047** — Do not declare protected members in sealed types
- [x] **CA1050** — Declare types in namespaces
- [x] **CA1051** — Do not declare visible instance fields
- [x] **CA1052** — Static holder types should be sealed
- [x] **CA1070** — Do not declare event fields as virtual

#### Naming (CA17xx)
- [x] **CA1707** — Identifiers should not contain underscores
- [x] **CA1712** — Do not prefix enum values with type name
- [x] **CA1714** — Flags enums should have plural names
- [x] **CA1715** — Identifiers should have correct prefix (covered by CSLINT101 + CSLINT106; pragma alias added)
- [x] **CA1716** — Identifiers should not match keywords
- [x] **CA1720** — Identifiers should not contain type names
- [x] **CA1721** — Property names should not match get methods
- [x] **CA1727** — Use PascalCase for named placeholders

#### Performance (CA18xx)
- [x] **CA1825** — Avoid zero-length array allocations (detect `new T[0]`)
- [x] **CA1861** — Avoid constant arrays as arguments

#### Reliability (CA20xx)
- [x] **CA2011** — Do not assign property within its setter
- [x] **CA2014** — Do not use stackalloc in loops
- [x] **CA2019** — ThreadStatic fields should not use inline initialization

#### Usage (CA21xx)
- [x] **CA2200** — Rethrow to preserve stack details (detect `throw ex;` in catch)
- [x] **CA2211** — Non-constant fields should not be visible
- [x] **CA2217** — Do not mark enums with FlagsAttribute (values not powers of 2)
- [x] **CA2219** — Do not raise exceptions in exception clauses
- [x] **CA2244** — Do not duplicate indexed element initializations
- [x] **CA2245** — Do not assign a property to itself
- [x] **CA2253** — Named placeholders should not be numeric values
- [x] **CA2259** — Ensure ThreadStatic is only used with static fields

### Implement remaining expression-bodied and null-checking rules

- [x] **IDE0022** — block body for methods, paired with IDE0021 (`csharp_style_expression_bodied_methods`)
- [x] **IDE0023** — expression-bodied conversion operators (`csharp_style_expression_bodied_operators`)
- [x] **IDE0024** — expression-bodied operators (`csharp_style_expression_bodied_operators`)
- [x] **IDE0026** — expression-bodied indexers (`csharp_style_expression_bodied_indexers`)
- [x] **IDE0027** — expression-bodied accessors (`csharp_style_expression_bodied_accessors`)
- [ ] **IDE0031** — null propagation (`dotnet_style_null_propagation`)
- [ ] **IDE0041** — prefer is null (`dotnet_style_prefer_is_null_check_over_reference_equality_method`)
- [x] **Merge CSLINT234 (InferredMemberNameRule) into IDE0037 (InferredTupleNamesRule)** — both map to IDE0037 (`dotnet_style_prefer_inferred_tuple_names` and `dotnet_style_prefer_inferred_anonymous_type_member_names`). Merge into a single IDE0037 rule covering both config keys, then drop CSLINT234

### Rename CSLINT rules with SA* equivalents to use SA IDs

- [ ] ⚠️ **BREAKING CHANGE (major version bump)** — **Rename CSLINT rules to SA IDs** — rules that map 1:1 to a StyleCop SA* diagnostic should use the SA ID directly as their `RuleId`, matching the pattern already established for IDE* and CA* rules. Old CSLINT IDs become pragma aliases for backward compatibility.
  - **1:1 mappings (37 rules):** CSLINT001→SA1028, CSLINT002→SA1027, CSLINT004→SA1518, CSLINT006→SA1124, CSLINT008→SA1507, CSLINT009→SA1517, CSLINT010→SA1412, CSLINT101→SA1302, CSLINT102→SA1300, CSLINT105→SA1303, CSLINT106→SA1314, CSLINT204→SA1101, CSLINT228→SA1503, CSLINT240→SA1106, CSLINT241→SA1107, CSLINT242→SA1131, CSLINT243→SA1132, CSLINT244→SA1133, CSLINT245→SA1134, CSLINT246→SA1136, CSLINT247→SA1505, CSLINT248→SA1508, CSLINT249→SA1509, CSLINT250→SA1516, CSLINT251→SA1401, CSLINT252→SA1402, CSLINT253→SA1413, CSLINT254→SA1000, CSLINT255→SA1001, CSLINT256→SA1002, CSLINT257→SA1003, CSLINT258→SA1005, CSLINT261→SA1024, CSLINT262→SA1025, CSLINT264→SA1214, CSLINT265→SA1203, CSLINT266→SA1204, CSLINT267→SA1202, CSLINT268→SA1201
  - **Multi-SA mappings (6 rules, need to pick primary ID):** CSLINT103→SA1312+SA1313, CSLINT104→SA1304+SA1306+SA1307+SA1311, CSLINT259→SA1008+SA1009, CSLINT260→SA1012+SA1013, CSLINT263→SA1212+SA1213, CSLINT269→SA1208+SA1209+SA1210+SA1211+SA1216+SA1217
