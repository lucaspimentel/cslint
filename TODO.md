# TODO

## Respect `#pragma warning disable` directives

Support suppressing CsLint diagnostics via `#pragma warning disable` directives, both with CsLint rule IDs (e.g., `CSLINT104`) and mapped third-party IDs (e.g., StyleCop's `SA1313`).

### Motivation

Some codebases intentionally suppress specific warnings with pragmas. For example, in dd-trace-dotnet:

```csharp
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
internal static string[] ParseMultipartIdentifier(..., bool ThrowOnEmptyMultipartName)
#pragma warning restore SA1313
```

CsLint should respect these suppressions to avoid flagging code the author has explicitly opted out of.

### Implementation plan

1. **Parse pragma directives from the syntax tree.** Roslyn already provides `SyntaxTree.GetDiagnostics()` pragma info, but more directly, the syntax tree contains `PragmaWarningDirectiveTriviaSyntax` nodes accessible via `root.DescendantTrivia()`. Each has:
   - `DisableOrRestoreKeyword` — whether it's a disable or restore
   - `ErrorCodes` — list of warning IDs being suppressed

2. **Build a suppression map in `FileLinter`.** Before running rules, scan the tree for pragma directives and build a structure mapping `(ruleId, lineRange)` pairs to suppressed regions. A `#pragma warning disable X` without a matching restore suppresses until end-of-file.

3. **Filter diagnostics post-analysis.** After collecting all diagnostics, remove any whose `RuleId` and `Line` fall within a suppressed region. This keeps the filtering centralized in `FileLinter` rather than spreading it across individual rules.

4. **Add a rule ID mapping table.** Create a static mapping from third-party rule IDs to CsLint IDs:
   - `SA1313` -> `CSLINT103` (parameter naming)
   - `SA1306` -> `CSLINT104` (field naming)
   - `SA1300` -> `CSLINT102` (member naming)
   - `IDE1006` -> `CSLINT102`, `CSLINT103`, `CSLINT104` (general naming)
   - etc.

   This allows `#pragma warning disable SA1313` to suppress `CSLINT103` diagnostics. The mapping only needs to cover commonly used IDs; unmapped IDs are simply ignored.

5. **Also support `CSLINT*` IDs directly.** Users should be able to write `#pragma warning disable CSLINT104` to suppress CsLint-specific rules.

### Rough code sketch

```csharp
// In FileLinter, after collecting diagnostics:
var suppressions = PragmaSuppressionMap.Build(root);
diagnostics.RemoveAll(d => suppressions.IsSuppressed(d.RuleId, d.Line));
```

```csharp
public class PragmaSuppressionMap
{
    // Maps ruleId -> list of (startLine, endLine) suppressed ranges
    private readonly Dictionary<string, List<(int Start, int End)>> _suppressions;

    public static PragmaSuppressionMap Build(CSharpSyntaxNode root) { ... }
    public bool IsSuppressed(string ruleId, int line) { ... }
}
```

## Review Microsoft code analysis rules

Sift through the rules in [Microsoft's docs](https://raw.githubusercontent.com/dotnet/docs/refs/heads/live/docs/fundamentals/code-analysis/categories.md) and decide which ones to implement in CsLint.
