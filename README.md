# CsLint

[![CI](https://github.com/lucaspimentel/cslint/actions/workflows/ci.yml/badge.svg)](https://github.com/lucaspimentel/cslint/actions/workflows/ci.yml) [![Release](https://github.com/lucaspimentel/cslint/actions/workflows/release.yml/badge.svg)](https://github.com/lucaspimentel/cslint/actions/workflows/release.yml)

A fast C# linter that respects `.editorconfig`. Uses Roslyn syntax-only parsing for fast single-file linting, with opt-in semantic analysis (`--semantic`) for deeper checks.

Pre-built binaries support Windows and Linux. macOS is supported when built from source or installed as a dotnet tool.

## Why?

`dotnet format --verify-no-changes` can be slow because it loads the full Roslyn Workspaces layer with semantic analysis. CsLint skips most of that — it parses syntax trees directly and reads rules from `.editorconfig`, making it fast enough to run as a hook on every file edit. When you need deeper analysis, the `--semantic` flag enables Tier 4 rules that use the Roslyn semantic model.

## Installation

### Scoop (Windows)

```bash
scoop bucket add lucaspimentel https://github.com/lucaspimentel/scoop-bucket
scoop install cslint
```

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
# Lint current directory (default if no path specified)
cslint

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

# Enable semantic analysis (Tier 4 rules)
cslint src/ --semantic

# List all available rules
cslint --list-rules

# Show resolved .editorconfig settings for a path
cslint --show-config .
cslint --show-config src/MyFile.cs
```

### Exit codes

| Code | Meaning |
|------|---------|
| 0    | No violations found |
| 1    | Violations found |
| 2    | Error (bad path, etc.) |

## Supported Rules

CsLint implements a subset of rules from Microsoft's [.NET code analysis framework](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/categories), including IDE code style analyzers and StyleCop (SA) rules. Rules are read from your `.editorconfig` and organized into four tiers:

### Tier 1 — Formatting
Text-level checks: indentation, line endings, whitespace, max line length, `#region`, file headers.

### Tier 2 — Naming
Naming conventions: PascalCase types/members, `I`-prefix interfaces, camelCase parameters, `_camelCase` fields.

### Tier 3 — Style
Code style preferences: `var` usage, expression-bodied members, braces, namespaces, pattern matching, sealed types, and more.

### Tier 4 — Semantic (requires `--semantic`)
Rules that use the Roslyn semantic model: unused usings, unused locals, unreachable code, duplicate enum values, self-assignment, unnecessary casts, redundant await.

See [docs/rule-mappings.md](docs/rule-mappings.md) for the complete rule reference with editorconfig keys and analyzer ID mappings.

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
| `SA1302` | `CSLINT101` | Interface prefix |
| `SA1313` | `CSLINT103` | Parameter naming |
| `SA1306` | `CSLINT104` | Field naming |
| `IDE0007`/`IDE0008` | `CSLINT200` | `var` preference |
| `IDE0160`/`IDE0161` | `CSLINT203` | Namespace declarations |
| `IDE1006` | `CSLINT102`, `CSLINT103`, `CSLINT104` | General naming |

See [docs/rule-mappings.md](docs/rule-mappings.md) for the full alias mapping table.

## Claude Code Integration

CsLint can run as a [Claude Code hook](https://docs.anthropic.com/en/docs/claude-code/hooks) to lint `.cs` files automatically after every edit. Make sure `cslint` is installed and available on your PATH (see [Installation](#installation)), then choose one of the options below.

### Option 1: Install the linters plugin

The [linters plugin](https://github.com/lucaspimentel/claude-plugins/tree/main/plugins/linters) sets up `PostToolUse` hooks for CsLint and other linters automatically. First add the marketplace, then install the plugin:

From the CLI:

```sh
claude plugin marketplace add https://github.com/lucaspimentel/claude-plugins
claude plugin install linters@lucasp-claude-plugins
```

Or from inside Claude Code:

```
/plugin marketplace add https://github.com/lucaspimentel/claude-plugins
/plugin install linters@lucasp-claude-plugins
```

### Option 2: Manual hook setup

Add a `PostToolUse` hook in `~/.claude/settings.json`:

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
