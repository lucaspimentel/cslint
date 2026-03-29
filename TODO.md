# TODO

## Low priority / high cost

- [ ] **CA1852 project-wide type hierarchy** — two-pass architecture to reduce false positives on `SealedTypePreferenceRule` by building an in-memory `HashSet<string>` of inherited type names before the lint pass, so base classes aren't flagged. **Rule is force-disabled until this is implemented.**
  - Requires a pre-scan pass in `DirectoryLinter` over all syntax trees to collect `BaseListSyntax` identifiers
  - New `IProjectContext` (or similar) threaded through `RuleContext` to rules that need cross-file info
  - Trade-off: simple-name matching only (no semantic model), so name collisions could suppress diagnostics on unrelated types — rare in practice
  - Single-file mode (`FileLinter.LintFile`) would fall back to current behavior (flag everything)
  - Main cost is architectural: breaks the clean file-at-a-time design for marginal gain on an `Info`-severity rule

## Standard .editorconfig rule coverage

Goal: support as many standard .editorconfig rules as possible using standard key names and diagnostic ID aliases. See [docs/rule-coverage-analysis.md](docs/rule-coverage-analysis.md) and [docs/standard-editorconfig-rules.md](docs/standard-editorconfig-rules.md) for full details.

### Implement missing Tier 3 style preference rules (require semantic analysis)

- [ ] **`dotnet_style_prefer_foreach_explicit_cast_in_source`** (IDE0220) — requires semantic analysis (need collection element type); Tier 4 candidate
- [ ] **`csharp_style_unused_value_expression_statement_preference`** (IDE0058) — requires semantic analysis (need return type info); Tier 4 candidate
- [ ] **`csharp_style_unused_value_assignment_preference`** (IDE0059) — requires semantic analysis (need data flow analysis); Tier 4 candidate
