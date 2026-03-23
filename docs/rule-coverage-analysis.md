# CsLint Rule Coverage vs Standard .editorconfig Rules

Analysis of which standard C# .editorconfig rules CsLint supports, and how.

---

## 1. Supported with Standard Name (exact match)

These CsLint rules use the same .editorconfig key as the standard .NET/C# convention.

### Tier 1 — Universal EditorConfig

| CsLint ID | Rule | Standard Key |
|-----------|------|-------------|
| CSLINT001 | TrailingWhitespace | `trim_trailing_whitespace` |
| CSLINT002 | Indentation | `indent_style`, `indent_size` |
| CSLINT003 | LineEnding | `end_of_line` |
| CSLINT004 | FinalNewline | `insert_final_newline` |
| CSLINT005 | MaxLineLength | `max_line_length` |
| CSLINT007 | FileHeader | `file_header_template` |

### Tier 3 — Code Style (exact standard key)

| CsLint ID | Rule | Standard Key(s) | Standard Rule ID |
|-----------|------|----------------|-----------------|
| CSLINT200 | VarPreference | `csharp_style_var_for_built_in_types`, `csharp_style_var_when_type_is_apparent`, `csharp_style_var_elsewhere` | IDE0007/IDE0008 |
| CSLINT201 | ExpressionBodied | `csharp_style_expression_bodied_methods`, `csharp_style_expression_bodied_properties`, `csharp_style_expression_bodied_accessors` | IDE0021–IDE0027 |
| CSLINT202 | BracePreference | `csharp_prefer_braces` | IDE0011 |
| CSLINT203 | NamespaceDeclaration | `csharp_style_namespace_declarations` | IDE0160/IDE0161 |
| CSLINT204 | ThisQualification | `dotnet_style_qualification_for_field`, `_property`, `_method`, `_event` | IDE0003/IDE0009 |
| CSLINT205 | ModifierOrder | `csharp_preferred_modifier_order` | IDE0036 |
| CSLINT206 | AccessibilityModifier | `dotnet_style_require_accessibility_modifiers` | IDE0040 |
| CSLINT207 | UsingDirectivePlacement | `csharp_using_directive_placement` | IDE0065 |
| CSLINT208 | PredefinedType | `dotnet_style_predefined_type_for_locals_parameters_members`, `dotnet_style_predefined_type_for_member_access` | IDE0049 |
| CSLINT209 | PatternMatching | `csharp_style_pattern_matching_over_is_with_cast_check` | IDE0020 |
| CSLINT211 | UsingDeclaration | `csharp_prefer_simple_using_statement` | IDE0063 |
| CSLINT212 | TargetTypedNew | `csharp_style_implicit_object_creation_when_type_is_apparent` | IDE0090 |
| CSLINT213 | SimpleDefaultExpression | `csharp_prefer_simple_default_expression` | IDE0034 |
| CSLINT214 | CompoundAssignment | `dotnet_style_prefer_compound_assignment` | IDE0054/IDE0074 |
| CSLINT215 | ObjectInitializer | `dotnet_style_object_initializer` | IDE0017 |
| CSLINT216 | CollectionInitializer | `dotnet_style_collection_initializer` | IDE0028 |
| CSLINT217 | ExpressionBodiedLambdas | `csharp_style_expression_bodied_lambdas` | IDE0053 |
| CSLINT218 | ExpressionBodiedLocalFunctions | `csharp_style_expression_bodied_local_functions` | IDE0061 |
| CSLINT219 | PatternMatchingNot | `csharp_style_prefer_not_pattern` | IDE0083 |
| CSLINT220 | PatternMatchingCombinator | `csharp_style_prefer_pattern_matching` | IDE0078 |
| CSLINT221 | PrimaryConstructor | `csharp_style_prefer_primary_constructors` | IDE0290 |
| CSLINT222 | CollectionExpression | `dotnet_style_prefer_collection_expression` | IDE0300–IDE0306 |
| CSLINT223 | TupleSwap | `csharp_style_prefer_tuple_swap` | IDE0180 |
| CSLINT224 | Utf8StringLiteral | `csharp_style_prefer_utf8_string_literals` | IDE0230 |
| CSLINT225 | SimplifyInterpolation | `dotnet_style_prefer_simplified_interpolation` | IDE0071 |
| CSLINT226 | IndexOperator | `csharp_style_prefer_index_operator` | IDE0056 |
| CSLINT227 | RangeOperator | `csharp_style_prefer_range_operator` | IDE0057 |
| CSLINT234 | InferredMemberName | `dotnet_style_prefer_inferred_anonymous_type_member_names` | IDE0037 |
| CSLINT235 | SimplifiedBooleanExpression | `dotnet_style_prefer_simplified_boolean_expressions` | IDE0075 |
| CSLINT236 | ExtendedPropertyPattern | `csharp_style_prefer_extended_property_pattern` | IDE0170 |

**Total: 36 rules using standard keys**

---

## 2. Supported with Custom Name (similar to a standard rule, but uses a non-standard key)

These CsLint rules cover functionality that has a standard .editorconfig key, but CsLint uses its own key name instead.

### Tier 1

| CsLint ID | Rule | CsLint Key | Standard Key | Standard Rule ID | Notes |
|-----------|------|-----------|-------------|-----------------|-------|
| CSLINT008 | MultipleBlankLines | `csharp_no_multiple_blank_lines` | `dotnet_style_allow_multiple_blank_lines_experimental` | IDE2000 | CsLint uses boolean opt-in; standard uses `true`/`false` with `_experimental` suffix |
| CSLINT010 | Utf8FileEncoding | `csharp_store_files_as_utf8` | `charset = utf-8` | (universal) | Standard uses `charset` key; CsLint uses custom boolean key |

### Tier 2 — Naming Rules

CsLint uses simplified single-key naming rules instead of the standard 3-part `dotnet_naming_rule` / `dotnet_naming_symbols` / `dotnet_naming_style` system. These keys look like standard naming rule names but are not actually parsed using the standard naming convention system.

| CsLint ID | Rule | CsLint Key | Standard Equivalent | Notes |
|-----------|------|-----------|-------------------|-------|
| CSLINT100 | TypeNaming | `dotnet_naming_rule.types_should_be_pascal_case` | `dotnet_naming_rule` + `dotnet_naming_symbols` + `dotnet_naming_style` (3-part system) | CsLint treats the full key as a single boolean toggle; standard requires defining symbol groups, styles, and rule severity separately |
| CSLINT101 | InterfacePrefix | `dotnet_naming_rule.interface_should_begin_with_i` | (same 3-part system) | Same — simplified single-key toggle |
| CSLINT102 | MemberNaming | `dotnet_naming_rule.members_should_be_pascal_case` | (same 3-part system) | Same |
| CSLINT103 | ParameterLocalNaming | `dotnet_naming_rule.locals_should_be_camel_case` | (same 3-part system) | Same |
| CSLINT104 | FieldNaming | `dotnet_naming_rule.private_fields_should_be_underscore_camel_case` | (same 3-part system) | Same |
| CSLINT105 | ConstantNaming | `dotnet_naming_rule.constants_should_be_pascal_case` | (same 3-part system) | Same |
| CSLINT106 | TypeParameterNaming | `dotnet_naming_rule.type_parameters_should_begin_with_t` | (same 3-part system) | Same |

### Tier 3 — Experimental blank line rules (now accept both keys)

CsLint accepts both its own key (without `_experimental` suffix) and the standard key (with `_experimental` suffix). The CsLint key takes precedence if both are present.

| CsLint ID | Rule | CsLint Key | Standard Key (also accepted) | Standard Rule ID |
|-----------|------|-----------|-------------|-----------------|
| CSLINT228 | EmbeddedStatement | `csharp_style_allow_embedded_statements_on_same_line` | `csharp_style_allow_embedded_statements_on_same_line_experimental` | IDE2001 |
| CSLINT229 | ConsecutiveBraces | `csharp_style_allow_blank_lines_between_consecutive_braces` | `csharp_style_allow_blank_lines_between_consecutive_braces_experimental` | IDE2002 |
| CSLINT231 | ConstructorInitializerBlankLine | `csharp_style_allow_blank_line_after_colon_in_constructor_initializer` | `csharp_style_allow_blank_line_after_colon_in_constructor_initializer_experimental` | IDE2004 |
| CSLINT232 | ConditionalExpressionBlankLine | `csharp_style_allow_blank_line_after_token_in_conditional_expression` | `csharp_style_allow_blank_line_after_token_in_conditional_expression_experimental` | IDE2005 |
| CSLINT233 | ArrowExpressionBlankLine | `csharp_style_allow_blank_line_after_token_in_arrow_expression_clause` | `csharp_style_allow_blank_line_after_token_in_arrow_expression_clause_experimental` | IDE2006 |

### Tier 3 — Different key name for similar concept

| CsLint ID | Rule | CsLint Key | Standard Key | Standard Rule ID |
|-----------|------|-----------|-------------|-----------------|
| CSLINT210 | NullChecking | `dotnet_style_null_checking` | `dotnet_style_null_propagation` (IDE0031), `dotnet_style_coalesce_expression` (IDE0029), `dotnet_style_prefer_is_null_check_over_reference_equality_method` (IDE0041) | IDE0029/IDE0031/IDE0041 |
| CSLINT230 | BlankLineAfterBlock | `csharp_style_allow_blank_line_after_block` | `dotnet_style_allow_statement_immediately_after_block_experimental` (also accepted) | IDE2003 |

**Total: 16 rules using custom keys (6 now also accept the standard key)**

---

## 3. Not Supported — Standard Rules with No CsLint Equivalent

### Would be Tier 1 (text-level)

None — CsLint covers all universal EditorConfig properties.

### Would be Tier 2 (naming)

| Standard Key / System | Standard Rule ID | Description |
|-----------------------|-----------------|-------------|
| `dotnet_naming_rule` / `dotnet_naming_symbols` / `dotnet_naming_style` (full 3-part system) | IDE1006 | CsLint has hardcoded naming rules but does NOT support the full configurable naming convention system (custom symbol groups, arbitrary styles, prefixes/suffixes, word separators, etc.) |

### Would be Tier 3 (style/syntax preferences)

| Standard Key | Standard Rule ID | Description |
|-------------|-----------------|-------------|
| `dotnet_style_readonly_field` | IDE0044 | Add readonly modifier |
| `csharp_style_pattern_matching_over_as_with_null_check` | IDE0019 | Pattern matching over `as` with null check |
| `csharp_style_throw_expression` | IDE0016 | Use throw expression |
| `csharp_style_inlined_variable_declaration` | IDE0018 | Inline variable declaration |
| `csharp_style_deconstructed_variable_declaration` | IDE0042 | Deconstruct variable declaration |
| `csharp_style_prefer_local_over_anonymous_function` | IDE0039 | Use local function instead of lambda |
| `csharp_style_conditional_delegate_call` | IDE1005 | Use conditional delegate call |
| `csharp_style_prefer_method_group_conversion` | IDE0200 | Remove unnecessary lambda expression |
| `csharp_style_prefer_top_level_statements` | IDE0210/IDE0211 | Top-level statements preference |
| `csharp_style_prefer_readonly_struct` | IDE0250 | Struct can be made readonly |
| `csharp_style_prefer_readonly_struct_member` | IDE0251 | Member can be made readonly |
| `csharp_style_prefer_null_check_over_type_check` | IDE0150 | Prefer null check over type check |
| `csharp_prefer_static_local_function` | IDE0062 | Make local function static |
| `csharp_prefer_static_anonymous_function` | IDE0320 | Make anonymous function static |
| `csharp_prefer_system_threading_lock` | IDE0330 | Prefer System.Threading.Lock |
| `csharp_style_prefer_unbound_generic_type_in_nameof` | IDE0340 | Use unbound generic type |
| `csharp_style_prefer_implicitly_typed_lambda_expression` | IDE0350 | Use implicitly typed lambda |
| `csharp_style_prefer_simple_property_accessors` | IDE0360 | Simplify property accessor |
| `csharp_style_unused_value_expression_statement_preference` | IDE0058 | Remove unused expression value |
| `csharp_style_unused_value_assignment_preference` | IDE0059 | Remove unnecessary value assignment |
| `dotnet_code_quality_unused_parameters` | IDE0060 | Remove unused parameter |
| `dotnet_style_prefer_auto_properties` | IDE0032 | Use auto property |
| `dotnet_style_prefer_conditional_expression_over_assignment` | IDE0045 | Conditional expression for assignment |
| `dotnet_style_prefer_conditional_expression_over_return` | IDE0046 | Conditional expression for return |
| `dotnet_style_prefer_inferred_tuple_names` | IDE0037 | Inferred tuple names |
| `dotnet_style_explicit_tuple_names` | IDE0033 | Use explicitly provided tuple name |
| `dotnet_style_prefer_foreach_explicit_cast_in_source` | IDE0220 | Add explicit cast in foreach |
| `dotnet_style_namespace_match_folder` | IDE0130 | Namespace match folder structure |
| `dotnet_style_parentheses_in_arithmetic_binary_operators` | IDE0047/IDE0048 | Parentheses preferences |
| `dotnet_style_parentheses_in_relational_binary_operators` | IDE0047/IDE0048 | Parentheses preferences |
| `dotnet_style_parentheses_in_other_binary_operators` | IDE0047/IDE0048 | Parentheses preferences |
| `dotnet_style_parentheses_in_other_operators` | IDE0047/IDE0048 | Parentheses preferences |
| `csharp_style_prefer_switch_expression` | IDE0066 | Use switch expression |
| `dotnet_style_allow_multiple_blank_lines_experimental` | IDE2000 | (CsLint has CSLINT008 with different key) |
| `csharp_style_allow_blank_line_after_colon_in_constructor_initializer_experimental` | IDE2004 | (now accepted by CSLINT231) |
| `csharp_style_allow_blank_line_after_token_in_conditional_expression_experimental` | IDE2005 | (now accepted by CSLINT232) |
| `csharp_style_allow_blank_line_after_token_in_arrow_expression_clause_experimental` | IDE2006 | (now accepted by CSLINT233) |
| `dotnet_style_allow_statement_immediately_after_block_experimental` | IDE2003 | (now accepted by CSLINT230) |
| `csharp_style_allow_embedded_statements_on_same_line_experimental` | IDE2001 | (now accepted by CSLINT228) |
| `csharp_style_allow_blank_lines_between_consecutive_braces_experimental` | IDE2002 | (now accepted by CSLINT229) |

### Would be Tier 3 (formatting — IDE0055)

| Standard Key | Description |
|-------------|-------------|
| `csharp_new_line_before_open_brace` | New line before open brace |
| `csharp_new_line_before_else` | New line before else |
| `csharp_new_line_before_catch` | New line before catch |
| `csharp_new_line_before_finally` | New line before finally |
| `csharp_new_line_before_members_in_object_initializers` | New line before members in object initializers |
| `csharp_new_line_before_members_in_anonymous_types` | New line before members in anonymous types |
| `csharp_new_line_between_query_expression_clauses` | New line between query expression clauses |
| `csharp_indent_case_contents` | Indent case contents |
| `csharp_indent_switch_labels` | Indent switch labels |
| `csharp_indent_labels` | Indent labels |
| `csharp_indent_block_contents` | Indent block contents |
| `csharp_indent_braces` | Indent braces |
| `csharp_indent_case_contents_when_block` | Indent case contents when block |
| `csharp_space_after_cast` | Space after cast |
| `csharp_space_after_keywords_in_control_flow_statements` | Space after keywords in control flow |
| `csharp_space_between_parentheses` | Space between parentheses |
| `csharp_space_before_colon_in_inheritance_clause` | Space before colon in inheritance |
| `csharp_space_after_colon_in_inheritance_clause` | Space after colon in inheritance |
| `csharp_space_around_binary_operators` | Space around binary operators |
| `csharp_space_between_method_declaration_parameter_list_parentheses` | Space in method declaration params |
| `csharp_space_between_method_declaration_empty_parameter_list_parentheses` | Space in empty method declaration params |
| `csharp_space_between_method_declaration_name_and_open_parenthesis` | Space between method name and parenthesis |
| `csharp_space_between_method_call_parameter_list_parentheses` | Space in method call params |
| `csharp_space_between_method_call_empty_parameter_list_parentheses` | Space in empty method call params |
| `csharp_space_between_method_call_name_and_opening_parenthesis` | Space between call name and parenthesis |
| `csharp_space_after_comma` | Space after comma |
| `csharp_space_before_comma` | Space before comma |
| `csharp_space_after_dot` | Space after dot |
| `csharp_space_before_dot` | Space before dot |
| `csharp_space_after_semicolon_in_for_statement` | Space after semicolon in for |
| `csharp_space_before_semicolon_in_for_statement` | Space before semicolon in for |
| `csharp_space_around_declaration_statements` | Space around declarations |
| `csharp_space_before_open_square_brackets` | Space before open square brackets |
| `csharp_space_between_empty_square_brackets` | Space between empty square brackets |
| `csharp_space_between_square_brackets` | Space between square brackets |
| `csharp_preserve_single_line_statements` | Preserve single line statements |
| `csharp_preserve_single_line_blocks` | Preserve single line blocks |
| `dotnet_sort_system_directives_first` | Sort System directives first |
| `dotnet_separate_import_directive_groups` | Separate import directive groups |

### Would be Tier 4 (semantic analysis)

These standard rules already have CsLint equivalents but CsLint uses `dotnet_diagnostic.CSLINT###.severity` keys, which is the standard severity mechanism (not a custom key). The following standard rules have **no** CsLint equivalent at all:

| Standard Rule ID | Description |
|-----------------|-------------|
| IDE0001 | Simplify name |
| IDE0002 | Simplify member access |
| IDE0010 | Add missing cases to switch statement |
| IDE0035 | Remove unreachable code (partially — CsLint has CSLINT302 UnreachableCode) |
| IDE0050 | Convert anonymous type to tuple |
| IDE0064 | Make struct fields writable |
| IDE0070 | Use System.HashCode.Combine |
| IDE0072 | Add missing cases to switch expression |
| IDE0076 | Remove invalid global SuppressMessageAttribute |
| IDE0077 | Avoid legacy format target in global SuppressMessageAttribute |
| IDE0079 | Remove unnecessary suppression |
| IDE0080 | Remove unnecessary suppression operator |
| IDE0082 | Convert typeof to nameof |
| IDE0100 | Remove unnecessary equality operator |
| IDE0110 | Remove unnecessary discard |
| IDE0120 | Simplify LINQ expression |
| IDE0121 | Simplify LINQ type check and cast |
| IDE0240 | Nullable directive is redundant |
| IDE0241 | Nullable directive is unnecessary |
| IDE0280 | Use nameof |
| IDE0370 | Remove unnecessary suppression |
| IDE0380 | Remove unnecessary unsafe modifier |

---

## Summary

| Category | Count |
|----------|-------|
| Supported with standard key | 36 |
| Supported with custom key (standard equivalent exists) | 16 |
| Standard rules not supported (style/syntax — would be Tier 3) | ~33 + ~39 formatting |
| Standard rules not supported (other IDE rules) | ~22 |
| Standard naming system not supported (configurable 3-part) | 1 (the full system) |
