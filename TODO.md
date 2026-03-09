# TODO

## Bugs to fix

Found via linting dd-trace-dotnet (`Datadog.Trace`, excluding `Vendors/`). Full analysis: [dd-trace-dotnet-lint-analysis.md](dd-trace-dotnet-lint-analysis.md)

- [x] **P0 — CSLINT105: local constants flagged as naming violations (~77 false positives)**
  `ConstantNamingRule.VisitLocalDeclarationStatement` enforces PascalCase/UPPER_CASE on local `const` variables inside method bodies. Local constants conventionally use camelCase (same as local variables). The rule should only enforce naming on class-level `const` fields, not local declarations.

- [x] **P1 — CSLINT104: struct fields in `[StructLayout]` types flagged (~10 false positives)**
  `FieldNamingRule` flags private fields in P/Invoke interop structs (e.g., `MEMORYSTATUSEX` with fields like `dwLength`, `ullTotalPhys`). These field names must match native Win32 APIs — renaming to `_camelCase` would break marshalling. The rule should skip types annotated with `[StructLayout]`.

- [x] **P2 — CSLINT210: false positive null-coalescing suggestion on ternary expressions (~4+ false positives)**
  The rule suggests `??` for patterns like `Resource != null ? Resource.GetHashCode() : 0`, but `??` doesn't apply here — the non-null branch produces an `int` (via method call), not the original nullable reference. Only suggest `??` when the ternary directly returns the checked variable in the true branch.

## Future rule candidates

Full analysis: [docs/rule-mappings.md — Future Candidates](docs/rule-mappings.md#future-candidates)
