# TODO

## Future rule candidates

Full analysis: [docs/rule-mappings.md — Future Candidates](docs/rule-mappings.md#future-candidates)

- [x] **CA1821 — Remove empty finalizers** (Tier 3, syntax-only) — implemented as CSLINT237
- [ ] **CA1805 — Do not initialize unnecessarily** (Tier 3, syntax-only, partial coverage)
  - Flag explicit default-value initializations where the type is stated: `int x = 0`, `bool b = false`, `string? s = null`, `object? o = null`, `double d = 0.0`, etc.
  - Only works when the type is explicit on the left-hand side (not `var`) — no semantic model to resolve inferred types
  - Implement as `CSharpSyntaxWalker` visiting field/local variable declarations
  - Map known type keywords to their default literals (`int`→`0`, `bool`→`false`, reference types→`null`, `default`)
