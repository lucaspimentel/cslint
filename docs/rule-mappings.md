# CsLint Rule Mappings

Comprehensive reference for all CsLint rules and their corresponding third-party rule IDs. CsLint rule IDs map to Microsoft's [IDE code style analyzers](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/categories) and StyleCop (SA) rules where applicable.

## Implemented Rules

### Tier 1 -- Formatting (text-level)

| CsLint ID | Description | editorconfig Key | Third-Party IDs |
|---|---|---|---|
| CSLINT001 | Trailing whitespace | `trim_trailing_whitespace` | SA1028 |
| CSLINT002 | Indentation | `indent_style`, `indent_size` | SA1027 |
| CSLINT003 | Line endings | `end_of_line` | -- |
| CSLINT004 | Final newline | `insert_final_newline` | SA1518 |
| CSLINT005 | Max line length | `max_line_length` | -- |
| CSLINT006 | No `#region` directives | `csharp_no_region_directives` | SA1124 |
| IDE0073 | File header | `file_header_template` | IDE0073 |
| CSLINT008 | No multiple blank lines | `csharp_no_multiple_blank_lines` | SA1507, IDE2000 |
| CSLINT009 | No blank lines at start of file | `csharp_no_blank_lines_at_start_of_file` | SA1517 |
| CSLINT010 | Store files as UTF-8 | `csharp_store_files_as_utf8` | SA1412 |

### Tier 2 -- Naming Conventions

| CsLint ID | Description | editorconfig Key | Third-Party IDs |
|---|---|---|---|
| CSLINT100 | Type naming (PascalCase) | `dotnet_naming_rule` | -- |
| CSLINT101 | Interface prefix (`I`) | `dotnet_naming_rule` | SA1302 |
| CSLINT102 | Member naming (PascalCase) | `dotnet_naming_rule` | SA1300, IDE1006 |
| CSLINT103 | Parameter/local naming (camelCase) | `dotnet_naming_rule` | SA1312, SA1313, IDE1006 |
| CSLINT104 | Field naming (`_camelCase`) | `dotnet_naming_rule` | SA1306, IDE1006 |
| CSLINT105 | Constant field naming (PascalCase/UPPER_CASE) | `dotnet_naming_rule` | SA1303 |
| CSLINT106 | Type parameter naming (T prefix) | `dotnet_naming_rule` | SA1314 |
| IDE1006 | Standard naming conventions (3-part system) | `dotnet_naming_rule.*`, `dotnet_naming_symbols.*`, `dotnet_naming_style.*` | IDE1006 |

### Tier 3 -- Style Preferences

| CsLint ID | Description | editorconfig Key | Third-Party IDs |
|---|---|---|---|
| IDE0007 | Prefer `var` | `csharp_style_var_when_type_is_apparent`, `csharp_style_var_for_built_in_types` | IDE0007 |
| IDE0008 | Prefer explicit type | `csharp_style_var_for_built_in_types`, `csharp_style_var_elsewhere` | IDE0008 |
| IDE0021 | Expression-bodied methods | `csharp_style_expression_bodied_methods` | IDE0021 |
| IDE0025 | Expression-bodied properties | `csharp_style_expression_bodied_properties` | IDE0025 |
| IDE0011 | Brace preference | `csharp_prefer_braces` | SA1500, IDE0011 |
| CSLINT203 | Namespace declarations | `csharp_style_namespace_declarations` | IDE0160, IDE0161 |
| CSLINT204 | `this.` qualification | `dotnet_style_qualification_for_*` | SA1101, IDE0003, IDE0009 |
| IDE0036 | Modifier ordering | `csharp_preferred_modifier_order` | SA1206, IDE0036 |
| IDE0040 | Accessibility modifiers | `dotnet_style_require_accessibility_modifiers` | SA1400, IDE0040 |
| IDE0065 | Using directive placement | `csharp_using_directive_placement` | IDE0065 |
| IDE0049 | Predefined type preferences | `dotnet_style_predefined_type_for_*` | SA1121, IDE0049 |
| CSLINT209 | Pattern matching | `csharp_style_pattern_matching_*` | IDE0020, IDE0038 |
| IDE0016 | Throw expression | `csharp_style_throw_expression` | IDE0016 |
| IDE0029 | Null coalescing | `dotnet_style_coalesce_expression` | IDE0029 |
| IDE0063 | Using declarations (`using var`) | `csharp_prefer_simple_using_statement` | IDE0063 |
| IDE0090 | Target-typed `new` | `csharp_style_implicit_object_creation_when_type_is_apparent` | IDE0090 |
| IDE0034 | Simplify `default` expression | `csharp_prefer_simple_default_expression` | IDE0034 |
| CSLINT214 | Compound assignment | `dotnet_style_prefer_compound_assignment` | IDE0054, IDE0074 |
| IDE0017 | Object initializers | `dotnet_style_object_initializer` | IDE0017 |
| IDE0028 | Collection initializers | `dotnet_style_collection_initializer` | IDE0028 |
| IDE0053 | Expression body for lambdas | `csharp_style_expression_bodied_lambdas` | IDE0053 |
| IDE0061 | Expression body for local functions | `csharp_style_expression_bodied_local_functions` | IDE0061 |
| CSLINT219 | Pattern matching (`not`) | `csharp_style_prefer_not_pattern` | IDE0083 |
| CSLINT220 | Pattern matching (`and`/`or`) | `csharp_style_prefer_pattern_matching` | IDE0078 |
| CSLINT221 | Primary constructors | `csharp_style_prefer_primary_constructors` | IDE0290 |
| CSLINT222 | Collection expressions | `dotnet_style_prefer_collection_expression` | IDE0300--IDE0305 |
| IDE0180 | Tuple swap | `csharp_style_prefer_tuple_swap` | IDE0180 |
| IDE0230 | UTF-8 string literals | `csharp_style_prefer_utf8_string_literals` | IDE0230 |
| IDE0071 | Simplify interpolation | `dotnet_style_prefer_simplified_interpolation` | IDE0071 |
| IDE0056 | Index operator (`^`) | `csharp_style_prefer_index_operator` | IDE0056 |
| IDE0057 | Range operator (`..`) | `csharp_style_prefer_range_operator` | IDE0057 |
| CSLINT228 | Embedded statements on own line | `csharp_style_allow_embedded_statements_on_same_line` | SA1503, IDE2001 |
| CSLINT229 | No blank line between consecutive braces | `csharp_style_allow_blank_lines_between_consecutive_braces` | IDE2002 |
| CSLINT230 | Blank line required after block | `csharp_style_allow_blank_line_after_block` | IDE2003 |
| CSLINT231 | No blank line after constructor initializer colon | `csharp_style_allow_blank_line_after_colon_in_constructor_initializer` | IDE2004 |
| CSLINT232 | No blank line after conditional expression token | `csharp_style_allow_blank_line_after_token_in_conditional_expression` | IDE2005 |
| CSLINT233 | No blank line after arrow expression token | `csharp_style_allow_blank_line_after_token_in_arrow_expression_clause` | IDE2006 |
| IDE0037 | Inferred member name | `dotnet_style_prefer_inferred_tuple_names`, `dotnet_style_prefer_inferred_anonymous_type_member_names` | IDE0037 |
| IDE0075 | Simplify boolean expression | `dotnet_style_prefer_simplified_boolean_expressions` | IDE0075 |
| IDE0170 | Extended property pattern | `csharp_style_prefer_extended_property_pattern` | IDE0170 |
| CA1821 | Empty finalizer | `csharp_no_empty_finalizers` | CA1821 |
| CA1805 | Do not initialize unnecessarily | `csharp_no_unnecessary_initialization` | CA1805 |
| CA1852 | Prefer sealed types | `csharp_prefer_sealed_types` | CA1852 |
| CA2011 | Do not assign property within its setter | `dotnet_diagnostic.CA2011.severity` | CA2011 |
| CA2014 | Do not use stackalloc in loops | `dotnet_diagnostic.CA2014.severity` | CA2014 |
| CA2200 | Rethrow to preserve stack details | `dotnet_diagnostic.CA2200.severity` | CA2200 |
| CA2219 | Do not raise exceptions in finally clauses | `dotnet_diagnostic.CA2219.severity` | CA2219 |
| CA1012 | Abstract types should not have public constructors | `dotnet_diagnostic.CA1012.severity` | CA1012 |
| CA1047 | Do not declare protected members in sealed types | `dotnet_diagnostic.CA1047.severity` | CA1047 |
| CA1052 | Static holder types should be sealed | `dotnet_diagnostic.CA1052.severity` | CA1052 |
| CA1070 | Do not declare event fields as virtual | `dotnet_diagnostic.CA1070.severity` | CA1070 |
| CA1041 | Provide ObsoleteAttribute message | `dotnet_diagnostic.CA1041.severity` | CA1041 |
| CA2019 | ThreadStatic fields should not use inline initialization | `dotnet_diagnostic.CA2019.severity` | CA2019 |
| CA2259 | Ensure ThreadStatic is only used with static fields | `dotnet_diagnostic.CA2259.severity` | CA2259 |
| CA1034 | Nested types should not be visible | `dotnet_diagnostic.CA1034.severity` | CA1034 |
| CA1040 | Avoid empty interfaces | `dotnet_diagnostic.CA1040.severity` | CA1040 |
| CA1044 | Properties should not be write only | `dotnet_diagnostic.CA1044.severity` | CA1044 |
| CA1050 | Declare types in namespaces | `dotnet_diagnostic.CA1050.severity` | CA1050 |
| CA1051 | Do not declare visible instance fields | `dotnet_diagnostic.CA1051.severity` | CA1051 |
| CA2211 | Non-constant fields should not be visible | `dotnet_diagnostic.CA2211.severity` | CA2211 |
| CSLINT240 | No empty statements | `csharp_no_empty_statements` | SA1106 |
| CSLINT241 | Single statement per line | `csharp_single_statement_per_line` | SA1107 |
| CSLINT242 | No Yoda conditions | `csharp_no_yoda_conditions` | SA1131 |
| CSLINT243 | No combined field declarations | `csharp_no_combined_field_declarations` | SA1132 |
| CSLINT244 | No combined attributes | `csharp_no_combined_attributes` | SA1133 |
| CSLINT245 | Attributes on own line | `csharp_attributes_on_own_line` | SA1134 |
| CSLINT246 | Enum values on separate lines | `csharp_enum_values_on_separate_lines` | SA1136 |
| CSLINT247 | No blank line after opening brace | `csharp_no_blank_line_after_opening_brace` | SA1505 |
| CSLINT248 | No blank line before closing brace | `csharp_no_blank_line_before_closing_brace` | SA1508 |
| CSLINT249 | No blank line before opening brace | `csharp_no_blank_line_before_opening_brace` | SA1509 |
| CSLINT250 | Elements separated by blank line | `csharp_elements_separated_by_blank_line` | SA1516 |
| CSLINT251 | Fields must be private | `csharp_fields_must_be_private` | SA1401 |
| CSLINT252 | Single type per file | `csharp_single_type_per_file` | SA1402 |
| CSLINT253 | Trailing commas in multi-line initializers | `csharp_trailing_commas_in_multi_line_initializers` | SA1413 |
| CSLINT254 | Keyword spacing | `csharp_keyword_spacing` | SA1000 |
| CSLINT255 | Comma spacing | `csharp_comma_spacing` | SA1001 |
| CSLINT256 | Semicolon spacing | `csharp_semicolon_spacing` | SA1002 |
| CSLINT257 | Operator spacing | `csharp_operator_spacing` | SA1003 |
| CSLINT258 | Single-line comment spacing | `csharp_comment_spacing` | SA1005 |
| CSLINT259 | Parenthesis spacing | `csharp_parenthesis_spacing` | SA1008, SA1009 |
| CSLINT260 | Brace spacing | `csharp_brace_spacing` | SA1012, SA1013 |
| CSLINT261 | Colon spacing | `csharp_colon_spacing` | SA1024 |
| CSLINT262 | No multiple whitespace | `csharp_no_multiple_whitespace` | SA1025 |
| CSLINT263 | Accessor ordering | `csharp_accessor_ordering` | SA1212, SA1213 |
| CSLINT264 | Readonly fields before mutable | `csharp_readonly_before_mutable` | SA1214 |
| CSLINT265 | Constants before fields | `csharp_constants_before_fields` | SA1203 |
| CSLINT266 | Static members before instance | `csharp_static_before_instance` | SA1204 |
| CSLINT267 | Element access modifier ordering | `csharp_element_access_ordering` | SA1202 |
| CSLINT268 | Element kind ordering | `csharp_element_ordering` | SA1201 |
| CSLINT269 | Using directive ordering | `csharp_using_directive_ordering` | SA1208, SA1209, SA1210, SA1211, SA1216, SA1217 |
| IDE0019 | Pattern matching over `as` with null check | `csharp_style_pattern_matching_over_as_with_null_check` | IDE0019 |
| IDE1005 | Conditional delegate call | `csharp_style_conditional_delegate_call` | IDE1005 |
| IDE0018 | Inlined variable declaration | `csharp_style_inlined_variable_declaration` | IDE0018 |
| IDE0066 | Switch expression preference | `csharp_style_prefer_switch_expression` | IDE0066 |
| IDE0045 | Conditional expression over assignment | `dotnet_style_prefer_conditional_expression_over_assignment` | IDE0045 |
| IDE0046 | Conditional expression over return | `dotnet_style_prefer_conditional_expression_over_return` | IDE0046 |
| IDE0039 | Local function over anonymous function | `csharp_style_prefer_local_over_anonymous_function` | IDE0039 |
| CSLINT277 | Sort System directives first | `dotnet_sort_system_directives_first` | -- |
| CSLINT278 | Separate import directive groups | `dotnet_separate_import_directive_groups` | -- |
| IDE0130 | Namespace match folder | `dotnet_style_namespace_match_folder` | IDE0130 |
| IDE0200 | Method group conversion | `csharp_style_prefer_method_group_conversion` | IDE0200 |
| IDE0210 | Top-level statements | `csharp_style_prefer_top_level_statements` | IDE0210 |
| CSLINT279 | New line before open brace | `csharp_new_line_before_open_brace` | -- |
| CSLINT280 | New line before else | `csharp_new_line_before_else` | -- |
| CSLINT281 | New line before catch | `csharp_new_line_before_catch` | -- |
| CSLINT282 | New line before finally | `csharp_new_line_before_finally` | -- |
| CSLINT283 | New line in object initializers | `csharp_new_line_before_members_in_object_initializers` | -- |
| CSLINT284 | New line in anonymous types | `csharp_new_line_before_members_in_anonymous_types` | -- |
| CSLINT285 | New line in query expressions | `csharp_new_line_between_query_expression_clauses` | -- |
| CSLINT286 | Cast spacing | `csharp_space_after_cast` | -- |
| CSLINT287 | Method declaration spacing | `csharp_space_between_method_declaration_*` | -- |
| CSLINT288 | Method call spacing | `csharp_space_between_method_call_*` | -- |
| CSLINT289 | Dot spacing | `csharp_space_before_dot`, `csharp_space_after_dot` | -- |
| CSLINT290 | Square bracket spacing | `csharp_space_*_square_brackets` | -- |
| CSLINT291 | Declaration statement spacing | `csharp_space_around_declaration_statements` | -- |
| CSLINT292 | Indentation formatting | `csharp_indent_*` | -- |
| CSLINT293 | Preserve single-line | `csharp_preserve_single_line_*` | -- |
| CSLINT305 | Empty catch block | `csharp_no_empty_catch_blocks` | -- |
| IDE0033 | Explicit tuple names | `dotnet_style_explicit_tuple_names` | IDE0033 |
| IDE0340 | Unbound generic in nameof | `csharp_style_prefer_unbound_generic_type_in_nameof` | IDE0340 |
| IDE0062 | Prefer static local function | `csharp_prefer_static_local_function` | IDE0062 |
| IDE0320 | Prefer static anonymous function | `csharp_prefer_static_anonymous_function` | IDE0320 |
| IDE0150 | Prefer null check over type check | `csharp_style_prefer_null_check_over_type_check` | IDE0150 |
| IDE0330 | Prefer System.Threading.Lock | `csharp_prefer_system_threading_lock` | IDE0330 |
| IDE0350 | Prefer implicitly typed lambda | `csharp_style_prefer_implicitly_typed_lambda_expression` | IDE0350 |
| IDE0360 | Prefer simple property accessors | `csharp_style_prefer_simple_property_accessors` | IDE0360 |

### Tier 4 -- Semantic Analysis (requires `--semantic`)

These rules use the Roslyn semantic model and are only active when the `--semantic` flag is passed.

| CsLint ID | Description | editorconfig Key | Third-Party IDs |
|---|---|---|---|
| IDE0005 | Unused using directive | `dotnet_diagnostic.IDE0005.severity` | IDE0005, CS8019 |
| CSLINT301 | Unused local variable | `dotnet_diagnostic.CSLINT301.severity` | CS0219 |
| CSLINT302 | Unreachable code | `dotnet_diagnostic.CSLINT302.severity` | CS0162 |
| CA1069 | Duplicate enum values | `dotnet_diagnostic.CA1069.severity` | CA1069 |
| CSLINT304 | Self-assignment | `dotnet_diagnostic.CSLINT304.severity` | CS1717 |
| IDE0004 | Unnecessary cast | `dotnet_diagnostic.IDE0004.severity` | IDE0004 |
| CSLINT307 | Redundant await | `dotnet_diagnostic.CSLINT307.severity` | -- |
| IDE0051 | Remove unused private member | `dotnet_diagnostic.IDE0051.severity` | IDE0051, CS0169 |
| IDE0052 | Remove unread private member | `dotnet_diagnostic.IDE0052.severity` | IDE0052, CS0414 |

## Pragma Alias Support

The following third-party rule IDs are recognized in `#pragma warning disable` directives and mapped to CsLint rules:

| Third-Party ID | CsLint ID(s) | Source |
|---|---|---|
| SA1000 | CSLINT254 | StyleCop |
| SA1001 | CSLINT255 | StyleCop |
| SA1002 | CSLINT256 | StyleCop |
| SA1003 | CSLINT257 | StyleCop |
| SA1005 | CSLINT258 | StyleCop |
| SA1008, SA1009 | CSLINT259 | StyleCop |
| SA1012, SA1013 | CSLINT260 | StyleCop |
| SA1024 | CSLINT261 | StyleCop |
| SA1025 | CSLINT262 | StyleCop |
| SA1027 | CSLINT002 | StyleCop |
| SA1028 | CSLINT001 | StyleCop |
| SA1101 | CSLINT204 | StyleCop |
| SA1106 | CSLINT240 | StyleCop |
| SA1107 | CSLINT241 | StyleCop |
| SA1121 | IDE0049 | StyleCop |
| SA1124 | CSLINT006 | StyleCop |
| SA1131 | CSLINT242 | StyleCop |
| SA1132 | CSLINT243 | StyleCop |
| SA1133 | CSLINT244 | StyleCop |
| SA1134 | CSLINT245 | StyleCop |
| SA1136 | CSLINT246 | StyleCop |
| SA1201 | CSLINT268 | StyleCop |
| SA1202 | CSLINT267 | StyleCop |
| SA1203 | CSLINT265 | StyleCop |
| SA1204 | CSLINT266 | StyleCop |
| SA1206 | IDE0036 | StyleCop |
| SA1208, SA1209, SA1210, SA1211, SA1216, SA1217 | CSLINT269 | StyleCop |
| SA1212, SA1213 | CSLINT263 | StyleCop |
| SA1214 | CSLINT264 | StyleCop |
| SA1300 | CSLINT102 | StyleCop |
| SA1302 | CSLINT101 | StyleCop |
| SA1303 | CSLINT105 | StyleCop |
| SA1304 | CSLINT104 | StyleCop |
| SA1306 | CSLINT104 | StyleCop |
| SA1307 | CSLINT104 | StyleCop |
| SA1311 | CSLINT104 | StyleCop |
| SA1312 | CSLINT103 | StyleCop |
| SA1313 | CSLINT103 | StyleCop |
| SA1314 | CSLINT106 | StyleCop |
| SA1400 | IDE0040 | StyleCop |
| SA1401 | CSLINT251 | StyleCop |
| SA1402 | CSLINT252 | StyleCop |
| SA1412 | CSLINT010 | StyleCop |
| SA1413 | CSLINT253 | StyleCop |
| SA1500 | IDE0011 | StyleCop |
| SA1503 | CSLINT228 | StyleCop |
| SA1505 | CSLINT247 | StyleCop |
| SA1507 | CSLINT008 | StyleCop |
| SA1508 | CSLINT248 | StyleCop |
| SA1509 | CSLINT249 | StyleCop |
| SA1516 | CSLINT250 | StyleCop |
| SA1517 | CSLINT009 | StyleCop |
| SA1518 | CSLINT004 | StyleCop |
| IDE0003, IDE0009 | CSLINT204 | Microsoft |
| IDE0007 | IDE0007 | Microsoft |
| IDE0008 | IDE0008 | Microsoft |
| IDE0011 | IDE0011 | Microsoft |
| IDE0016 | IDE0016 | Microsoft |
| IDE0017 | IDE0017 | Microsoft |
| IDE0018 | IDE0018 | Microsoft |
| IDE0019 | IDE0019 | Microsoft |
| IDE0020, IDE0038 | CSLINT209 | Microsoft |
| IDE0021 | IDE0021 | Microsoft |
| IDE0025 | IDE0025 | Microsoft |
| IDE0028 | IDE0028 | Microsoft |
| IDE0029 | IDE0029 | Microsoft |
| IDE0034 | IDE0034 | Microsoft |
| IDE0036 | IDE0036 | Microsoft |
| IDE0039 | IDE0039 | Microsoft |
| IDE0040 | IDE0040 | Microsoft |
| IDE0045 | IDE0045 | Microsoft |
| IDE0046 | IDE0046 | Microsoft |
| IDE0049 | IDE0049 | Microsoft |
| IDE0053 | IDE0053 | Microsoft |
| IDE0054, IDE0074 | CSLINT214 | Microsoft |
| IDE0061 | IDE0061 | Microsoft |
| IDE0063 | IDE0063 | Microsoft |
| IDE0065 | IDE0065 | Microsoft |
| IDE0066 | IDE0066 | Microsoft |
| IDE0073 | IDE0073 | Microsoft |
| IDE0078 | CSLINT220 | Microsoft |
| IDE0083 | CSLINT219 | Microsoft |
| IDE0056 | IDE0056 | Microsoft |
| IDE0057 | IDE0057 | Microsoft |
| IDE0071 | IDE0071 | Microsoft |
| IDE0090 | IDE0090 | Microsoft |
| IDE0160, IDE0161 | CSLINT203 | Microsoft |
| IDE0180 | IDE0180 | Microsoft |
| IDE0230 | IDE0230 | Microsoft |
| IDE0290 | CSLINT221 | Microsoft |
| IDE0300--IDE0305 | CSLINT222 | Microsoft |
| IDE1005 | IDE1005 | Microsoft |
| IDE1006 | CSLINT102, CSLINT103, CSLINT104 | Microsoft |
| IDE2000 | CSLINT008 | Microsoft |
| IDE2001 | CSLINT228 | Microsoft |
| IDE2002 | CSLINT229 | Microsoft |
| IDE2003 | CSLINT230 | Microsoft |
| IDE2004 | CSLINT231 | Microsoft |
| IDE2005 | CSLINT232 | Microsoft |
| IDE2006 | CSLINT233 | Microsoft |
| IDE0037 | IDE0037 | Microsoft |
| IDE0051 | IDE0051 | Microsoft |
| IDE0052 | IDE0052 | Microsoft |
| IDE0075 | IDE0075 | Microsoft |
| IDE0170 | IDE0170 | Microsoft |
| CA1821 | CA1821 | Microsoft |
| CA1805 | CA1805 | Microsoft |
| CA1852 | CA1852 | Microsoft |
| IDE0004 | IDE0004 | Microsoft |
| IDE0005 | IDE0005 | Microsoft |
| CS0162 | CSLINT302 | C# compiler |
| CS0169 | IDE0051 | C# compiler |
| CS0219 | CSLINT301 | C# compiler |
| CS0414 | IDE0052 | C# compiler |
| CS1717 | CSLINT304 | C# compiler |
| CA1069 | CA1069 | Microsoft |
| CA2011 | CA2011 | Microsoft |
| CA2014 | CA2014 | Microsoft |
| CA2200 | CA2200 | Microsoft |
| CA2219 | CA2219 | Microsoft |
| CA1012 | CA1012 | Microsoft |
| CA1047 | CA1047 | Microsoft |
| CA1052 | CA1052 | Microsoft |
| CA1070 | CA1070 | Microsoft |
| CA1041 | CA1041 | Microsoft |
| CA2019 | CA2019 | Microsoft |
| CA2259 | CA2259 | Microsoft |
| CA1034 | CA1034 | Microsoft |
| CA1040 | CA1040 | Microsoft |
| CA1044 | CA1044 | Microsoft |
| CA1050 | CA1050 | Microsoft |
| CA1051 | CA1051 | Microsoft |
| CA2211 | CA2211 | Microsoft |

## Future Candidates

### Syntax-Feasible (planned)

These rules can be implemented with syntax-only parsing:

#### Stretch goals (partially feasible)

| IDE Rule | Description | Notes |
|---|---|---|
| IDE0047 | Remove unnecessary parentheses | Operator precedence is syntax-determinable but rules are complex |
| IDE0048 | Add parentheses for clarity | Inverse of IDE0047; equally complex |

### Not Feasible (require semantic model)

These rules require type information, flow analysis, or semantic model and are not currently implemented (some may become Tier 4 candidates in the future):

IDE0001, IDE0002, IDE0010, IDE0032, IDE0035, IDE0042, IDE0044, IDE0050, IDE0058, IDE0059, IDE0060, IDE0064, IDE0070, IDE0072, IDE0076, IDE0077, IDE0079, IDE0080, IDE0082, IDE0100, IDE0110, IDE0120, IDE0121, IDE0140, IDE0220, IDE0240, IDE0241, IDE0250, IDE0251, IDE0260, IDE0270, IDE0280, IDE0306, IDE0370, IDE0380

### Not Applicable

| IDE Rule | Reason |
|---|---|
| IDE0055 | General formatting — already covered by CsLint Tier 1 rules |
| IDE0081 | Remove `ByVal` — VB.NET only |
| IDE0084 | Use `IsNot` pattern — VB.NET only |
| IDE3000 | Implement with Copilot — not a linting rule |
