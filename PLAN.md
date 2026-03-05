# Cslint - Fast C# Linter Respecting .editorconfig

## Context

`dotnet format --verify-no-changes` is slow (~28s) because it uses Roslyn's full Workspaces layer with semantic analysis. For an AI agent hook that runs on every file edit, we need sub-100ms single-file linting. Cslint achieves this by using **Roslyn syntax-only parsing** (no compilation/semantic model) and reading rules from `.editorconfig`.

## Architecture

```
Cslint.Cli          (console app, System.CommandLine, exit codes)
  -> Cslint.Core    (rules, config, engine, formatters)
Cslint.Core.Tests   (xUnit + Moq)
```

**Key design choices:**
- **Roslyn syntax-only** (`CSharpSyntaxTree.ParseText`) -- no `Compilation`, no semantic model
- **editorconfig-core-net** NuGet for .editorconfig parsing (handles glob matching, section precedence, `root=true`)
- **Manual rule registration** in `RuleRegistry` (no reflection -- trim-safe)
- **`CSharpSyntaxWalker`** per rule for tree-based checks
- **`PublishAot`** for Native AOT compilation as a single-file executable
- **Exit codes**: 0 = clean, 1 = violations, 2 = error

## Project Structure

```
Cslint.slnx
Directory.Build.props              # net10.0, LangVersion=latest, Nullable=enable
.editorconfig                      # Dogfooding
src/
  Cslint.Core/
    Config/
      IConfigProvider.cs           # Abstraction over config source
      EditorConfigProvider.cs      # Wraps editorconfig-core-net, caches per directory
      LintConfiguration.cs         # Typed wrapper over raw key-value pairs
    Rules/
      IRuleDefinition.cs           # RuleId, Name, ConfigKeys, IsEnabled(), Analyze()
      RuleContext.cs               # FilePath, SourceText, SyntaxTree, Root, Configuration
      Diagnostic.cs                # LintDiagnostic: RuleId, Message, Severity, FilePath, Line, Column
      DiagnosticSeverity.cs        # LintSeverity: None, Info, Warning, Error
      Tier1/                       # Formatting (text-level, no syntax tree needed)
        IndentationRule.cs         # indent_style, indent_size
        LineEndingRule.cs          # end_of_line
        TrailingWhitespaceRule.cs  # trim_trailing_whitespace
        FinalNewlineRule.cs        # insert_final_newline
        MaxLineLengthRule.cs       # max_line_length
      Tier2/                       # Naming conventions (CSharpSyntaxWalker)
        TypeNamingRule.cs          # PascalCase for classes/structs/enums/records
        InterfacePrefixRule.cs     # I-prefix on interfaces
        MemberNamingRule.cs        # PascalCase for methods/properties/events
        ParameterLocalNamingRule.cs # camelCase for parameters/locals
        FieldNamingRule.cs         # _camelCase for private fields
        ConstantNamingRule.cs      # PascalCase or UPPER_CASE
      Tier3/                       # Style preferences (syntax tree analysis)
        VarPreferenceRule.cs       # IDE0007/IDE0008
        ExpressionBodiedRule.cs    # IDE0021-IDE0027
        BracePreferenceRule.cs     # IDE0011
        NamespaceDeclarationRule.cs # IDE0160/IDE0161
        ThisQualificationRule.cs   # IDE0003/IDE0009
        ModifierOrderRule.cs       # IDE0036
        AccessibilityModifierRule.cs # IDE0040
        UsingDirectivePlacementRule.cs # IDE0065
        PredefinedTypeRule.cs      # IDE0049
        PatternMatchingRule.cs     # IDE0019/IDE0020/IDE0066
        NullCheckingRule.cs        # IDE0029-IDE0031/IDE0041
    Engine/
      RuleRegistry.cs              # Explicit registration of all rules
      FileLinter.cs                # Parse file -> resolve config -> run enabled rules
      DirectoryLinter.cs           # Parallel.ForEachAsync over *.cs files
    Reporting/
      IOutputFormatter.cs
      TextFormatter.cs             # src/Foo.cs(12,5): warning IDE0007: Use 'var'...
      JsonFormatter.cs
      SarifFormatter.cs
  Cslint.Cli/
    Program.cs                     # System.CommandLine: cslint <path> [--format text|json|sarif]
test/
  Cslint.Core.Tests/               # [Theory]-based tests per rule, engine, formatters
```

## NuGet Dependencies

| Package | Project | Purpose |
|---------|---------|---------|
| Microsoft.CodeAnalysis.CSharp | Core | Syntax-only parsing |
| editorconfig | Core | .editorconfig file parsing |
| System.CommandLine | Cli | CLI argument parsing |
| xunit | Tests | Test framework |
| Moq | Tests | Mocking IConfigProvider |

## Implementation Phases

### Phase 1: Skeleton + First Rule ✅
1. Create `Cslint.slnx`, all `.csproj` files, `Directory.Build.props`, `.gitignore`, `.editorconfig`
2. Core types: `LintDiagnostic`, `LintSeverity`, `RuleContext`, `IRuleDefinition`
3. Config: `IConfigProvider`, `EditorConfigProvider`, `LintConfiguration`
4. First rule: `TrailingWhitespaceRule` (pure text, simplest possible)
5. Engine: `RuleRegistry`, `FileLinter`
6. Output: `TextFormatter`
7. CLI: `Program.cs` with System.CommandLine
8. Tests for `TrailingWhitespaceRule` + `FileLinter`
9. Verify end-to-end: `dotnet run -- some-file.cs`

### Phase 2: Remaining Tier 1 Rules ✅
- `IndentationRule`, `LineEndingRule`, `FinalNewlineRule`, `MaxLineLengthRule`
- All text-level checks, no syntax tree needed
- `[Theory]` tests for each

### Phase 3: Tier 2 Naming Rules ✅
- Shared `NamingHelper` utility (`IsPascalCase`, `IsCamelCase`, `HasPrefix`)
- All 6 naming rules using `CSharpSyntaxWalker`
- Parse `dotnet_naming_rule.*` config properties

### Phase 4: Tier 3 Style Rules ✅
- All 11 style rules
- `var` rule is approximate (syntax-only can detect `new`, casts, literals but not method return types -- false negatives OK, no false positives)

### Phase 5: Output Formats + Directory Linting ✅
- `JsonFormatter` (System.Text.Json)
- `SarifFormatter` (SARIF v2.1.0)
- `DirectoryLinter` with parallel processing
- Skip generated files (`*.g.cs`, `*.designer.cs`, `obj/`)

### Phase 6: Polish ✅
- `--severity` filter, `--exclude` glob option
- Performance benchmarking (target < 100ms single file)
- Native AOT publish profile

## Verification

1. `dotnet build` -- compiles without warnings
2. `dotnet test` -- all rule tests pass
3. `dotnet run --project src/Cslint.Cli -- src/Cslint.Core/` -- lints itself (dogfooding)
4. Single-file performance: `time cslint some-file.cs` < 100ms
5. Exit code 0 on clean file, exit code 1 on file with violations
