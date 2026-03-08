# CsLint Rule Mappings

Comprehensive reference for all CsLint rules and their corresponding third-party rule IDs. CsLint rule IDs map to Microsoft's [IDE code style analyzers](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/categories) and StyleCop (SA) rules where applicable.

## Implemented Rules

### Tier 1 -- Formatting (text-level)

| CsLint ID | Description | editorconfig Key | Third-Party IDs |
|---|---|---|---|
| CSLINT001 | Trailing whitespace | `trim_trailing_whitespace` | -- |
| CSLINT002 | Indentation | `indent_style`, `indent_size` | -- |
| CSLINT003 | Line endings | `end_of_line` | -- |
| CSLINT004 | Final newline | `insert_final_newline` | -- |
| CSLINT005 | Max line length | `max_line_length` | -- |
| CSLINT006 | No `#region` directives | `dotnet_diagnostic.CSLINT006.severity` | -- |
| CSLINT007 | File header | `file_header_template` | IDE0073 |
| CSLINT008 | No multiple blank lines | `csharp_no_multiple_blank_lines` | IDE2000 |

### Tier 2 -- Naming Conventions

| CsLint ID | Description | editorconfig Key | Third-Party IDs |
|---|---|---|---|
| CSLINT100 | Type naming (PascalCase) | `dotnet_naming_rule` | -- |
| CSLINT101 | Interface prefix (`I`) | `dotnet_naming_rule` | SA1302 |
| CSLINT102 | Member naming (PascalCase) | `dotnet_naming_rule` | SA1300, IDE1006 |
| CSLINT103 | Parameter/local naming (camelCase) | `dotnet_naming_rule` | SA1313, IDE1006 |
| CSLINT104 | Field naming (`_camelCase`) | `dotnet_naming_rule` | SA1306, IDE1006 |
| CSLINT105 | Constant naming (PascalCase/UPPER_CASE) | `dotnet_naming_rule` | -- |

### Tier 3 -- Style Preferences

| CsLint ID | Description | editorconfig Key | Third-Party IDs |
|---|---|---|---|
| CSLINT200 | `var` preference | `csharp_style_var_*` | IDE0007, IDE0008 |
| CSLINT201 | Expression-bodied members | `csharp_style_expression_bodied_*` | IDE0021--IDE0027 |
| CSLINT202 | Brace preference | `csharp_prefer_braces` | IDE0011 |
| CSLINT203 | Namespace declarations | `csharp_style_namespace_declarations` | IDE0160, IDE0161 |
| CSLINT204 | `this.` qualification | `dotnet_style_qualification_for_*` | IDE0003, IDE0009 |
| CSLINT205 | Modifier ordering | `csharp_preferred_modifier_order` | IDE0036 |
| CSLINT206 | Accessibility modifiers | `dotnet_style_require_accessibility_modifiers` | IDE0040 |
| CSLINT207 | Using directive placement | `csharp_using_directive_placement` | IDE0065 |
| CSLINT208 | Predefined type preferences | `dotnet_style_predefined_type_for_*` | IDE0049 |
| CSLINT209 | Pattern matching | `csharp_style_pattern_matching_*` | IDE0019, IDE0020, IDE0066 |
| CSLINT210 | Null checking | `csharp_style_*_null_check` | IDE0029--IDE0031, IDE0041 |
| CSLINT211 | Using declarations (`using var`) | `csharp_prefer_simple_using_statement` | IDE0063 |
| CSLINT212 | Target-typed `new` | `csharp_style_implicit_object_creation_when_type_is_apparent` | IDE0090 |
| CSLINT213 | Simplify `default` expression | `csharp_prefer_simple_default_expression` | IDE0034 |
| CSLINT214 | Compound assignment | `dotnet_style_prefer_compound_assignment` | IDE0054, IDE0074 |
| CSLINT215 | Object initializers | `dotnet_style_object_initializer` | IDE0017 |
| CSLINT216 | Collection initializers | `dotnet_style_collection_initializer` | IDE0028 |
| CSLINT217 | Expression body for lambdas | `csharp_style_expression_bodied_lambdas` | IDE0053 |
| CSLINT218 | Expression body for local functions | `csharp_style_expression_bodied_local_functions` | IDE0061 |
| CSLINT219 | Pattern matching (`not`) | `csharp_style_prefer_not_pattern` | IDE0083 |
| CSLINT220 | Pattern matching (`and`/`or`) | `csharp_style_prefer_pattern_matching` | IDE0078 |
| CSLINT221 | Primary constructors | `csharp_style_prefer_primary_constructors` | IDE0290 |
| CSLINT222 | Collection expressions | `dotnet_style_prefer_collection_expression` | IDE0300--IDE0305 |
| CSLINT223 | Tuple swap | `csharp_style_prefer_tuple_swap` | IDE0180 |
| CSLINT224 | UTF-8 string literals | `csharp_style_prefer_utf8_string_literals` | IDE0230 |
| CSLINT225 | Simplify interpolation | `dotnet_style_prefer_simplified_interpolation` | IDE0071 |
| CSLINT226 | Index operator (`^`) | `csharp_style_prefer_index_operator` | IDE0056 |
| CSLINT227 | Range operator (`..`) | `csharp_style_prefer_range_operator` | IDE0057 |
| CSLINT228 | Embedded statements on own line | `csharp_style_allow_embedded_statements_on_same_line` | IDE2001 |
| CSLINT229 | No blank line between consecutive braces | `csharp_style_allow_blank_lines_between_consecutive_braces` | IDE2002 |
| CSLINT230 | Blank line required after block | `csharp_style_allow_blank_line_after_block` | IDE2003 |
| CSLINT231 | No blank line after constructor initializer colon | `csharp_style_allow_blank_line_after_colon_in_constructor_initializer` | IDE2004 |
| CSLINT232 | No blank line after conditional expression token | `csharp_style_allow_blank_line_after_token_in_conditional_expression` | IDE2005 |
| CSLINT233 | No blank line after arrow expression token | `csharp_style_allow_blank_line_after_token_in_arrow_expression_clause` | IDE2006 |
| CSLINT234 | Inferred member name | `dotnet_style_prefer_inferred_anonymous_type_member_names` | IDE0037 |
| CSLINT235 | Simplify boolean expression | `dotnet_style_prefer_simplified_boolean_expressions` | IDE0075 |
| CSLINT236 | Extended property pattern | `csharp_style_prefer_extended_property_pattern` | IDE0170 |

## Pragma Alias Support

The following third-party rule IDs are recognized in `#pragma warning disable` directives and mapped to CsLint rules:

| Third-Party ID | CsLint ID(s) | Source |
|---|---|---|
| SA1300 | CSLINT102 | StyleCop |
| SA1302 | CSLINT101 | StyleCop |
| SA1306 | CSLINT104 | StyleCop |
| SA1313 | CSLINT103 | StyleCop |
| IDE0003, IDE0009 | CSLINT204 | Microsoft |
| IDE0007, IDE0008 | CSLINT200 | Microsoft |
| IDE0011 | CSLINT202 | Microsoft |
| IDE0017 | CSLINT215 | Microsoft |
| IDE0019, IDE0020, IDE0066 | CSLINT209 | Microsoft |
| IDE0021--IDE0027 | CSLINT201 | Microsoft |
| IDE0028 | CSLINT216 | Microsoft |
| IDE0029--IDE0031, IDE0041 | CSLINT210 | Microsoft |
| IDE0034 | CSLINT213 | Microsoft |
| IDE0036 | CSLINT205 | Microsoft |
| IDE0040 | CSLINT206 | Microsoft |
| IDE0049 | CSLINT208 | Microsoft |
| IDE0053 | CSLINT217 | Microsoft |
| IDE0054, IDE0074 | CSLINT214 | Microsoft |
| IDE0061 | CSLINT218 | Microsoft |
| IDE0063 | CSLINT211 | Microsoft |
| IDE0065 | CSLINT207 | Microsoft |
| IDE0073 | CSLINT007 | Microsoft |
| IDE0078 | CSLINT220 | Microsoft |
| IDE0083 | CSLINT219 | Microsoft |
| IDE0056 | CSLINT226 | Microsoft |
| IDE0057 | CSLINT227 | Microsoft |
| IDE0071 | CSLINT225 | Microsoft |
| IDE0090 | CSLINT212 | Microsoft |
| IDE0160, IDE0161 | CSLINT203 | Microsoft |
| IDE0180 | CSLINT223 | Microsoft |
| IDE0230 | CSLINT224 | Microsoft |
| IDE0290 | CSLINT221 | Microsoft |
| IDE0300--IDE0305 | CSLINT222 | Microsoft |
| IDE1006 | CSLINT102, CSLINT103, CSLINT104 | Microsoft |
| IDE2000 | CSLINT008 | Microsoft |
| IDE2001 | CSLINT228 | Microsoft |
| IDE2002 | CSLINT229 | Microsoft |
| IDE2003 | CSLINT230 | Microsoft |
| IDE2004 | CSLINT231 | Microsoft |
| IDE2005 | CSLINT232 | Microsoft |
| IDE2006 | CSLINT233 | Microsoft |
| IDE0037 | CSLINT234 | Microsoft |
| IDE0075 | CSLINT235 | Microsoft |
| IDE0170 | CSLINT236 | Microsoft |

## Future Candidates

### Syntax-Feasible (planned)

These rules can be implemented with syntax-only parsing:

#### Stretch goals (partially feasible)

| IDE Rule | Description | Notes |
|---|---|---|
| IDE0038 | Pattern matching (`is` + cast, no variable) | Overlaps with CSLINT209; requires cross-statement matching |
| IDE0047 | Remove unnecessary parentheses | Operator precedence is syntax-determinable but rules are complex |
| IDE0048 | Add parentheses for clarity | Inverse of IDE0047; equally complex |

### Not Feasible (require semantic model)

These rules are excluded because they need type information, flow analysis, or semantic model:

IDE0001, IDE0002, IDE0004, IDE0005, IDE0010, IDE0016, IDE0018, IDE0032, IDE0033, IDE0035, IDE0039, IDE0042, IDE0044, IDE0045, IDE0046, IDE0050, IDE0051, IDE0052, IDE0058, IDE0059, IDE0060, IDE0062, IDE0064, IDE0070, IDE0072, IDE0076, IDE0077, IDE0079, IDE0080, IDE0082, IDE0100, IDE0110, IDE0120, IDE0121, IDE0130, IDE0140, IDE0150, IDE0200, IDE0210, IDE0211, IDE0220, IDE0240, IDE0241, IDE0250, IDE0251, IDE0260, IDE0270, IDE0280, IDE0306, IDE0320, IDE0330, IDE0340, IDE0350, IDE0360, IDE0370, IDE0380, IDE1005

### Not Applicable

| IDE Rule | Reason |
|---|---|
| IDE0055 | General formatting — already covered by CsLint Tier 1 rules |
| IDE0081 | Remove `ByVal` — VB.NET only |
| IDE0084 | Use `IsNot` pattern — VB.NET only |
| IDE3000 | Implement with Copilot — not a linting rule |
