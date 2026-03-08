# CLAUDE.local.md

## Validation with Datadog.Trace

Use the `Datadog.Trace` project as a real-world validation corpus for CsLint rules.

### Target path

`D:\source\datadog\dd-trace-dotnet\tracer\src\Datadog.Trace`

### Run command

```bash
dotnet run --project src/CsLint.Cli -- "D:/source/datadog/dd-trace-dotnet/tracer/src/Datadog.Trace" --exclude "**/Vendors/**"
```

Add `--severity warning` to filter out info-level diagnostics.

### EditorConfig chain

Three `.editorconfig` files apply (in precedence order):

1. **`tracer/src/Datadog.Trace/.editorconfig`** — project-level overrides (currently just `DDSEAL001`)
2. **`tracer/src/.editorconfig`** — `CA2007` severity
3. **`.editorconfig`** (root, `root = true`) — main config with:
   - `indent_style = space`, `indent_size = 4`, `insert_final_newline = true`, `charset = utf-8`
   - `var` preference: all three `csharp_style_var_*` set to `true:suggestion`
   - Expression-bodied: methods/constructors/operators `false:none`, properties/indexers/accessors `true:none`
   - `this.` qualification: all four set to `false:suggestion`
   - Predefined types: both set to `true:suggestion`
   - Object/collection initializers: `true:suggestion`
   - Null coalescing/propagation: `true:suggestion`
   - Pattern matching: `true:suggestion`

To enable Priority 3 rules for validation, add keys to the project-level `.editorconfig`:

```ini
[*.cs]
dotnet_style_prefer_simplified_interpolation = true:suggestion
csharp_style_prefer_index_operator = true:suggestion
csharp_style_prefer_range_operator = true:suggestion
dotnet_style_prefer_collection_expression = true:suggestion
csharp_style_prefer_primary_constructors = true:suggestion
csharp_style_prefer_tuple_swap = true:suggestion
csharp_style_prefer_utf8_string_literals = true:suggestion
csharp_no_multiple_blank_lines = true
csharp_style_allow_embedded_statements_on_same_line = false:suggestion
csharp_style_allow_blank_lines_between_consecutive_braces = false:suggestion
csharp_style_allow_blank_line_after_block = false:suggestion
csharp_style_allow_blank_line_after_colon_in_constructor_initializer = false:suggestion
csharp_style_allow_blank_line_after_token_in_conditional_expression = false:suggestion
csharp_style_allow_blank_line_after_token_in_arrow_expression_clause = false:suggestion
dotnet_style_prefer_inferred_anonymous_type_member_names = true:suggestion
dotnet_style_prefer_simplified_boolean_expressions = true:suggestion
csharp_style_prefer_extended_property_pattern = true:suggestion
```

### Corpus stats

- ~3,082 `.cs` files (excluding `Vendors/`)
- With current config: 411 diagnostics (after bug fixes)
- With new rules enabled (P3 + CSLINT008/228–236): adds ~988 more diagnostics (1,399 total)

### Validated diagnostic counts per rule

#### Baseline (no P3 config)

| Rule | Count | Description |
|------|-------|-------------|
| CSLINT104 | 27 | Private field naming (_camelCase) |
| CSLINT105 | 77 | Constant naming (PascalCase/UPPER_CASE) |
| CSLINT200 | 164 | var preference (99 built-in, 65 type-apparent) |
| CSLINT201 | 36 | Expression-bodied properties |
| CSLINT204 | 55 | Remove this. qualification |
| CSLINT210 | 40 | Null coalescing operator |
| CSLINT215 | 8 | Object initializer |
| CSLINT216 | 4 | Collection initializer |
| **Total** | **411** | |

#### With P3 rules enabled (temporary editorconfig)

| Rule | Count | Description |
|------|-------|-------------|
| CSLINT221 | 266 | Primary constructor suggestion |
| CSLINT222 | 600 | Collection expression |
| CSLINT224 | 1 | UTF-8 string literal |
| CSLINT225 | 5 | Simplified interpolation |
| CSLINT226 | 27 | Index operator (^) |
| CSLINT227 | 38 | Range operator (..) |
| CSLINT008 | 9 | No multiple blank lines |
| CSLINT230 | 39 | Blank line after block statement |
| CSLINT235 | 2 | Simplified boolean expression |
| CSLINT236 | 1 | Extended property pattern |

#### Rules that did not fire (expected)

- **Clean codebase**: CSLINT100 (trailing whitespace), CSLINT102 (indent style), CSLINT103 (final newline), CSLINT106 (no regions)
- **No config**: CSLINT107 (max line length), CSLINT108/220 (file header), CSLINT109-113 (type/method/property/event/enum naming), CSLINT214 (using declaration), CSLINT219 (namespace declaration)
- **Severity none**: CSLINT202 (expression-bodied methods), CSLINT203 (expression-bodied constructors)
- **No violations**: CSLINT205-207 (predefined types for members, this. for methods/events), CSLINT211 (null propagation), CSLINT212 (explicit tuple names), CSLINT213 (inlined var), CSLINT217 (throw expression), CSLINT218 (conditional delegate), CSLINT223 (tuple swap), CSLINT228 (embedded statements on same line), CSLINT229 (blank lines between consecutive braces), CSLINT231 (blank line after colon in constructor initializer), CSLINT232 (blank line after token in conditional), CSLINT233 (blank line after token in arrow expression), CSLINT234 (inferred anonymous type member names)

### Bugs found and fixed

#### Bug 1: CSLINT101 — SA1302 pragma alias missing

SA1302 (StyleCop's "Interface names should begin with I") was not mapped to CSLINT101 in `PragmaAliasMap`. An interface suppressed with `#pragma warning disable SA1302` was still flagged.

**Fix**: Added `["SA1302"] = ["CSLINT101"]` to `PragmaAliasMap.cs`.

#### Bug 2: CSLINT200 — false positives for null/default/mismatched numeric literals

`IsTypeApparent()` included `LiteralExpressionSyntax`, causing `null`, `default`, and numeric literals to be treated as "type apparent". This produced false positives like:
- `SpanContext? parent = null;` — type NOT apparent from `null`
- `TraceId traceId = default;` — type NOT apparent from `default`
- `ulong spanId = 0;` — `var` would infer `int`, not `ulong`

**Fixes**:
1. Early-return for `null` and `default` literals (type can never be inferred)
2. Removed `LiteralExpressionSyntax` from `IsTypeApparent()` (literals are not "type apparent")
3. Added `LiteralMatchesDeclaredType()` check for `var_for_built_in_types` path — only suggests `var` when the literal's natural type matches the declared type (e.g., `int x = 42` ✓, `ulong x = 0` ✗)

**Impact**: Reduced CSLINT200 from 791 to 164 hits (627 false positives eliminated).

#### Bug 3: CSLINT208 — false positive on property/field named "String"

`PredefinedTypeRule` flagged any `IdentifierNameSyntax` matching a framework type name (e.g., `String`) unless its parent was a `MemberAccessExpressionSyntax`. Property assignments like `String = @string;` (where `String` is a property name) were falsely flagged.

**Fix**: Replaced the negative parent check with `IsInTypePosition()` — a positive check that only flags identifiers used in known type positions (variable declarations, parameters, return types, casts, etc.).

#### Bug 4: CSLINT209 — false positive when is-check and cast target different expressions

`PatternMatchingRule` checked for any `is Type` expression followed by any cast in the if-body, without verifying the expressions matched. Code like `if (info.Kind is SomeEnum) { var x = (OtherType)info; }` was falsely flagged.

**Fix**: Added expression matching — only flags when the `is` check target and the cast target are the same expression (compared via `ToString()`).

#### Bug 5: CSLINT216 — false positive on HashCode builder pattern

`CollectionInitializerRule` flagged `new HashCode()` followed by `.Add()` calls as candidates for collection initializer syntax. `HashCode` uses a builder pattern — its `.Add()` method is not a collection add, and `HashCode` doesn't support initializer syntax.

**Fix**: Added type exclusion for `HashCode` / `System.HashCode` in `CollectionInitializerRule.cs`.

**Impact**: Reduced CSLINT216 from 8 to 4 hits (4 false positives eliminated).

### Known acceptable patterns

- **CSLINT104**: P/Invoke struct fields (e.g., `dwLength`, `dwMemoryLoad` in `MEMORYSTATUSEX`) are real violations per naming conventions but intentionally follow Windows API naming. These should be suppressed with pragmas.
- **CSLINT105**: Some intentional `camelCase` constants in Datadog.Trace — real violations per editorconfig.
- **`#if` conditional compilation**: CsLint parses all code regardless of preprocessor directives — may flag code in inactive `#if` branches. This is expected/acceptable behavior.
- **Generated code**: `.g.cs` and `.designer.cs` files are auto-excluded by `DirectoryLinter`.
