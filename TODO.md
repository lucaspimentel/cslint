# TODO

## Map third-party rule IDs to CsLint IDs for pragma suppression

`#pragma warning disable CSLINTXXX` is already supported. Add a mapping table so third-party IDs (e.g., `SA1313`, `IDE1006`) also suppress the corresponding CsLint rules.

Proposed mappings:
- `SA1313` -> `CSLINT103` (parameter naming)
- `SA1306` -> `CSLINT104` (field naming)
- `SA1300` -> `CSLINT102` (member naming)
- `IDE1006` -> `CSLINT102`, `CSLINT103`, `CSLINT104` (general naming)

This would allow `#pragma warning disable SA1313` to suppress `CSLINT103` diagnostics. Unmapped IDs are ignored.

## Review Microsoft code analysis rules

Sift through the rules in [Microsoft's docs](https://raw.githubusercontent.com/dotnet/docs/refs/heads/live/docs/fundamentals/code-analysis/categories.md) and decide which ones to implement in CsLint.
