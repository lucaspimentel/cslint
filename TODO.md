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
- [ ] **Unnecessary casts** — detect redundant casts where both types are in-source or BCL (symbol resolution reliable)
- [ ] **Redundant await** — detect `async` methods that just `return await` a single call; `Task`/`ValueTask` are BCL so symbol resolution is reliable
- [ ] **Unused private members** — fields/methods/properties with `private` access declared but never referenced within the compilation
- [ ] **Duplicate enum values** — constant value analysis is purely local, no external type resolution needed
- [ ] **Self-assignment detection** (`x = x`) — symbol equality check, no external type resolution needed
- [ ] **Empty catch blocks** — catch blocks that swallow exceptions without logging/rethrowing; structural + symbol check on `Exception` (BCL type)

## Low priority / high cost

- [ ] **CSLINT239 project-wide type hierarchy** — two-pass architecture to reduce false positives on `SealedTypePreferenceRule` by building an in-memory `HashSet<string>` of inherited type names before the lint pass, so base classes aren't flagged
  - Requires a pre-scan pass in `DirectoryLinter` over all syntax trees to collect `BaseListSyntax` identifiers
  - New `IProjectContext` (or similar) threaded through `RuleContext` to rules that need cross-file info
  - Trade-off: simple-name matching only (no semantic model), so name collisions could suppress diagnostics on unrelated types — rare in practice
  - Single-file mode (`FileLinter.LintFile`) would fall back to current behavior (flag everything)
  - Main cost is architectural: breaks the clean file-at-a-time design for marginal gain on an `Info`-severity rule
