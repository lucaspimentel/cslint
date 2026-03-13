# TODO

## Future rule candidates

Full analysis: [docs/rule-mappings.md — Future Candidates](docs/rule-mappings.md#future-candidates)

- [ ] **CA1821 — Remove empty finalizers** (Tier 3, syntax-only)
  - Detect `~ClassName() { }` destructors with empty bodies
  - Implement as `CSharpSyntaxWalker` visiting `DestructorDeclarationSyntax` nodes
  - Check if body block has no statements (or only whitespace/comments)
  - Place in `Rules/Tier3/`, register in `RuleRegistry`
- [ ] **CA1805 — Do not initialize unnecessarily** (Tier 3, syntax-only, partial coverage)
  - Flag explicit default-value initializations where the type is stated: `int x = 0`, `bool b = false`, `string? s = null`, `object? o = null`, `double d = 0.0`, etc.
  - Only works when the type is explicit on the left-hand side (not `var`) — no semantic model to resolve inferred types
  - Implement as `CSharpSyntaxWalker` visiting field/local variable declarations
  - Map known type keywords to their default literals (`int`→`0`, `bool`→`false`, reference types→`null`, `default`)
