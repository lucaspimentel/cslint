# TODO

## Future rule candidates

Full analysis: [docs/rule-mappings.md — Future Candidates](docs/rule-mappings.md#future-candidates)

- [x] **CA1821 — Remove empty finalizers** (Tier 3, syntax-only) — implemented as CSLINT237
- [x] **CA1805 — Do not initialize unnecessarily** (Tier 3, syntax-only, partial coverage) — implemented as CSLINT238
- [x] **CA1852 — Prefer sealed types** (Tier 3, syntax-only) — implemented as CSLINT239

## Low priority / high cost

- [ ] **CSLINT239 project-wide type hierarchy** — two-pass architecture to reduce false positives on `SealedTypePreferenceRule` by building an in-memory `HashSet<string>` of inherited type names before the lint pass, so base classes aren't flagged
  - Requires a pre-scan pass in `DirectoryLinter` over all syntax trees to collect `BaseListSyntax` identifiers
  - New `IProjectContext` (or similar) threaded through `RuleContext` to rules that need cross-file info
  - Trade-off: simple-name matching only (no semantic model), so name collisions could suppress diagnostics on unrelated types — rare in practice
  - Single-file mode (`FileLinter.LintFile`) would fall back to current behavior (flag everything)
  - Main cost is architectural: breaks the clean file-at-a-time design for marginal gain on an `Info`-severity rule
