---
name: validate
description: >
  Run CsLint against an external codebase and analyze the results for actionable bugs
  (false positives, misclassifications, edge cases). Use when the user says "validate",
  "validate cslint", "run validation", "check for false positives", "test against",
  "lint and analyze", or wants to run CsLint against a target codebase to find bugs
  in CsLint itself. Also trigger when the user mentions analyzing lint results or
  finding false positives in CsLint rules.
---

# CsLint Validation

Run CsLint against an external codebase, then analyze the results to find actionable bugs in CsLint's rules.

## Parsing user input

The user provides natural language describing what to validate. Extract these from their input:

- **path** (required): The directory or file path to lint. This becomes CsLint's positional `<path>` argument.
- **exclude** (optional): A glob pattern to exclude. This becomes CsLint's `--exclude` option. The `--exclude` flag accepts a glob pattern, so convert the user's description into a proper glob:
  - User says "exclude Vendors" → `--exclude "Vendors/**"`
  - User says "skip test files" → `--exclude "**/test/**"`
  - User says "ignore generated code" → `--exclude "**/*.g.cs"`

Examples of natural language input and how to interpret them:
- `"validate against dd-trace-dotnet"` → path: `D:\source\datadog\dd-trace-dotnet` (resolve relative to common source directories)
- `"run cslint on ../other-repo excluding Vendors"` → path: `../other-repo`, exclude: `Vendors/**`
- `"validate D:\source\myproject, skip the generated folder"` → path: `D:\source\myproject`, exclude: `**/generated/**`

If the path is ambiguous, ask the user to clarify before running.

## Step 1 — Run CsLint and analyze

Run CsLint with `--format json`, save the output, then run the analysis script:

```bash
dotnet run --project src/CsLint.Cli -- --format json <path> [--exclude <exclude-pattern>] 2>/dev/null > cslint-validate.json
```

Write the JSON output to the current directory (not `$TEMP` — that path doesn't work reliably on Windows with Git Bash). Clean up `cslint-validate.json` when done.

CsLint exits 0 (clean), 1 (violations found), or 2 (error). Exit code 1 is expected — it means violations were found, which is what we want to analyze. If exit code is 2, report the error and stop.

Then run the bundled analysis script to get a summary and flag suspicious patterns:

```bash
python "${SKILL_DIR}/scripts/analyze.py" cslint-validate.json
```

`${SKILL_DIR}` is the directory containing this SKILL.md file.

The script prints:
- Total violation count and breakdown by rule ID
- Top 5 most common messages per rule
- Suspicious patterns flagged automatically (verbatim identifiers, empty names, PascalCase params, generated files)

To get file:line details for specific rules (useful for investigation):

```bash
python "${SKILL_DIR}/scripts/analyze.py" cslint-validate.json --details-only CSLINT210 CSLINT104
```

Use `--details-only` (not `--details`) to skip the summary and print only file:line violations — this works well with piping and `head`. Pass rule IDs to filter, or omit them to print all violations.

## Step 2 — Investigate suspicious patterns

Use `--details` on rules that look suspicious from the summary, then **read the actual source code** at the reported file:line to confirm whether it's a false positive. Read 3–5 representative examples per pattern — don't just guess from the message text.

### What to look for when reading source

**Naming rules (CSLINT100–CSLINT105):**
- Verbatim identifiers (`@` prefix) — indicates `.Text` vs `.ValueText` bug
- Empty identifier names — CsLint reading the wrong token
- PascalCase parameters that are actually primary constructor or record parameters
- Fields in interop/P/Invoke structs — names must match native APIs
- Local constants flagged by the class-level constant rule

**Modifier-sensitive rules (rules that should behave differently based on modifiers):**
- `const` fields flagged by rules that only apply to mutable fields (e.g., unnecessary initialization — constants *require* an initializer)
- `static readonly` fields flagged by instance-only rules
- Fields in structs flagged by class-only rules (e.g., "field should be private" — struct fields are commonly public for data carriers, interop, etc.)

**Context-sensitive rules (rules that should consider the containing type/scope):**
- Fields in `[StructLayout]` interop structs — must be public for marshaling
- Members in test classes or test harnesses — may follow different conventions
- Members in nested private types — encapsulation is already provided by the outer type

**Style rules (CSLINT200+):**
- Rule suggestion doesn't apply to the actual code pattern (e.g., suggesting `??` on a ternary that returns different types)
- Extremely high violation counts for a single rule vs others (outlier)

**Concentration pattern:**
- If >80% of a rule's violations come from <3 files, it often indicates a context the rule doesn't handle (interop files, generated code, lookup tables with alignment whitespace, etc.)

Group confirmed false positives by root cause. A single bug in CsLint can produce many false positives.

## Step 3 — Report

Present findings in two sections:

### Confirmed or likely bugs in CsLint
For each bug found:
- Which rule is affected
- How many false positives it produces
- Root cause (with example source snippets)
- Suggested fix

### Statistics (for context)
- Total violations by rule
- How many are suspected false positives vs likely real violations

Focus on actionable findings. If CsLint is working correctly on the target codebase, say so.
