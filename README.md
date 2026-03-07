# CsLint

A fast C# linter that respects `.editorconfig`. Uses Roslyn syntax-only parsing (no compilation or semantic model) for fast single-file linting.

## Why?

`dotnet format --verify-no-changes` is slow (~28s) because it loads the full Roslyn Workspaces layer with semantic analysis. CsLint skips all of that — it parses syntax trees directly and reads rules from `.editorconfig`, making it fast enough to run as a hook on every file edit.

## Installation

### .NET tool

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet tool install --global cslint
```

### Download pre-built binary

Requires PowerShell 7+.

```pwsh
irm https://raw.githubusercontent.com/lucaspimentel/cslint/main/install-remote.ps1 | iex
```

### Build from source

Requires PowerShell 7+ and [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```pwsh
git clone https://github.com/lucaspimentel/cslint
cd cslint
./install-local.ps1
```

## Usage

```bash
# Lint a single file
cslint path/to/File.cs

# Lint a directory (recursively)
cslint src/

# Output as JSON or SARIF
cslint src/ --format json
cslint src/ --format sarif

# Filter by minimum severity
cslint src/ --severity warning

# Exclude files by glob pattern
cslint src/ --exclude "**/Generated/*.cs" --exclude "**/*.g.cs"
```

### Exit codes

| Code | Meaning |
|------|---------|
| 0    | No violations found |
| 1    | Violations found |
| 2    | Error (bad path, etc.) |

## Supported Rules

Rules are read from your `.editorconfig` and organized into three tiers:

### Tier 1 — Formatting (text-level)
- **Indentation** — `indent_style`, `indent_size`
- **Line endings** — `end_of_line`
- **Trailing whitespace** — `trim_trailing_whitespace`
- **Final newline** — `insert_final_newline`
- **Max line length** — `max_line_length`
- **No `#region` directives** — `dotnet_diagnostic.CSLINT006.severity`
- **File header** — `file_header_template` (IDE0073)

### Tier 2 — Naming conventions
- Type naming (PascalCase for classes, structs, enums, records)
- Interface prefix (`I`-prefix)
- Member naming (PascalCase for methods, properties, events)
- Parameter/local naming (camelCase)
- Field naming (`_camelCase` for private fields)
- Constant naming (PascalCase or UPPER_CASE)

### Tier 3 — Style preferences
- `var` usage (IDE0007/IDE0008)
- Expression-bodied members (IDE0021–IDE0027)
- Brace preferences (IDE0011)
- Namespace declarations (IDE0160/IDE0161)
- `this.` qualification (IDE0003/IDE0009)
- Modifier ordering (IDE0036)
- Accessibility modifiers (IDE0040)
- `using` directive placement (IDE0065)
- Predefined type preferences (IDE0049)
- Pattern matching (IDE0019/IDE0020/IDE0066)
- Null checking (IDE0029–IDE0031/IDE0041)
- Using declarations (IDE0063)
- Target-typed `new` (IDE0090)
- Simplify `default` expression (IDE0034)
- Compound assignment (IDE0054/IDE0074)
- Object initializers (IDE0017)
- Collection initializers (IDE0028)
- Expression body for lambdas (IDE0053)
- Expression body for local functions (IDE0061)
- Pattern matching `not` (IDE0083)
- Pattern matching combinators `and`/`or` (IDE0078)
- Primary constructors (IDE0290)
- Collection expressions (IDE0300–IDE0305)
- Tuple swap (IDE0180)
- UTF-8 string literals (IDE0230)
- Simplify interpolation (IDE0071)
- Index operator `^` (IDE0056)
- Range operator `..` (IDE0057)

## Suppressing Diagnostics

Suppress specific CsLint rules with `#pragma warning disable`:

```csharp
#pragma warning disable CSLINT001
class Foo { }   // trailing whitespace not reported
#pragma warning restore CSLINT001
```

- Supports single or multiple rule IDs: `#pragma warning disable CSLINT001, CSLINT200`
- `#pragma warning disable` (no IDs) suppresses all CsLint rules in that range
- Without a matching `restore`, suppression continues to end of file
- Third-party rule IDs (StyleCop `SA*` and Microsoft `IDE*`) are also recognized and mapped to the corresponding CsLint rules. For example:

| Third-Party ID | CsLint ID(s) | Rule |
|---|---|---|
| `SA1313` | `CSLINT103` | Parameter naming |
| `SA1306` | `CSLINT104` | Field naming |
| `IDE0007`/`IDE0008` | `CSLINT200` | `var` preference |
| `IDE0160`/`IDE0161` | `CSLINT203` | Namespace declarations |
| `IDE1006` | `CSLINT102`, `CSLINT103`, `CSLINT104` | General naming |

See [docs/rule-mappings.md](docs/rule-mappings.md) for the full alias mapping table.

## Development

```bash
dotnet build
dotnet test
```

## Claude Code Integration

CsLint can run as a [Claude Code hook](https://docs.anthropic.com/en/docs/claude-code/hooks) to lint `.cs` files automatically after every edit. Add a `PostToolUse` hook in `~/.claude/settings.json`:

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "command": "~/.claude/hooks/cs-lint.sh"
          }
        ]
      }
    ]
  }
}
```

With `~/.claude/hooks/cs-lint.sh`:

```bash
#!/bin/bash
FILE_PATH=$(jq -r ".tool_input.file_path")
if [[ "$FILE_PATH" == *.cs ]]; then
  OUTPUT=$(cslint "$FILE_PATH" 2>&1)
  if [[ -n "$OUTPUT" ]]; then
    echo "$OUTPUT" >&2
    exit 2
  fi
fi
```

When a hook exits with code 2, Claude Code receives the output as feedback and can fix the violations automatically.

## License

This project is licensed under the [MIT License](LICENSE).
