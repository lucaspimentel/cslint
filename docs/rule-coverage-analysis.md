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
| IDE0073 | FileHeader | `file_header_template` | IDE0073 |

### Tier 2 — Naming (standard 3-part system)

| CsLint ID | Rule | Standard Key(s) | Standard Rule ID |
|-----------|------|----------------|-----------------|
| IDE1006 | NamingConvention | `dotnet_naming_rule.*`, `dotnet_naming_symbols.*`, `dotnet_naming_style.*` | IDE1006 |

### Tier 3 — Code Style (exact standard key)

| CsLint ID | Rule | Standard Key(s) | Standard Rule ID |
|-----------|------|----------------|-----------------|
| CSLINT200 | VarPreference | `csharp_style_var_for_built_in_types`, `csharp_style_var_when_type_is_apparent`, `csharp_style_var_elsewhere` | IDE0007/IDE0008 |
| CSLINT201 | ExpressionBodied | `csharp_style_expression_bodied_methods`, `csharp_style_expression_bodied_properties`, `csharp_style_expression_bodied_accessors` | IDE0021–IDE0027 |
| IDE0011 | BracePreference | `csharp_prefer_braces` | IDE0011 |
| CSLINT203 | NamespaceDeclaration | `csharp_style_namespace_declarations` | IDE0160/IDE0161 |
| CSLINT204 | ThisQualification | `dotnet_style_qualification_for_field`, `_property`, `_method`, `_event` | IDE0003/IDE0009 |
| IDE0036 | ModifierOrder | `csharp_preferred_modifier_order` | IDE0036 |
| IDE0040 | AccessibilityModifier | `dotnet_style_require_accessibility_modifiers` | IDE0040 |
| IDE0065 | UsingDirectivePlacement | `csharp_using_directive_placement` | IDE0065 |
| IDE0049 | PredefinedType | `dotnet_style_predefined_type_for_locals_parameters_members`, `dotnet_style_predefined_type_for_member_access` | IDE0049 |
| CSLINT209 | PatternMatching | `csharp_style_pattern_matching_over_is_with_cast_check` | IDE0020 |
| IDE0063 | UsingDeclaration | `csharp_prefer_simple_using_statement` | IDE0063 |
| IDE0090 | TargetTypedNew | `csharp_style_implicit_object_creation_when_type_is_apparent` | IDE0090 |
| IDE0034 | SimpleDefaultExpression | `csharp_prefer_simple_default_expression` | IDE0034 |
| CSLINT214 | CompoundAssignment | `dotnet_style_prefer_compound_assignment` | IDE0054/IDE0074 |
| IDE0017 | ObjectInitializer | `dotnet_style_object_initializer` | IDE0017 |
| IDE0028 | CollectionInitializer | `dotnet_style_collection_initializer` | IDE0028 |
| IDE0053 | ExpressionBodiedLambdas | `csharp_style_expression_bodied_lambdas` | IDE0053 |
| IDE0061 | ExpressionBodiedLocalFunctions | `csharp_style_expression_bodied_local_functions` | IDE0061 |
| CSLINT219 | PatternMatchingNot | `csharp_style_prefer_not_pattern` | IDE0083 |
| CSLINT220 | PatternMatchingCombinator | `csharp_style_prefer_pattern_matching` | IDE0078 |
| CSLINT221 | PrimaryConstructor | `csharp_style_prefer_primary_constructors` | IDE0290 |
| CSLINT222 | CollectionExpression | `dotnet_style_prefer_collection_expression` | IDE0300–IDE0306 |
| IDE0180 | TupleSwap | `csharp_style_prefer_tuple_swap` | IDE0180 |
| IDE0230 | Utf8StringLiteral | `csharp_style_prefer_utf8_string_literals` | IDE0230 |
| IDE0071 | SimplifyInterpolation | `dotnet_style_prefer_simplified_interpolation` | IDE0071 |
| IDE0056 | IndexOperator | `csharp_style_prefer_index_operator` | IDE0056 |
| IDE0057 | RangeOperator | `csharp_style_prefer_range_operator` | IDE0057 |
| CSLINT234 | InferredMemberName | `dotnet_style_prefer_inferred_anonymous_type_member_names` | IDE0037 |
| IDE0075 | SimplifiedBooleanExpression | `dotnet_style_prefer_simplified_boolean_expressions` | IDE0075 |
| IDE0170 | ExtendedPropertyPattern | `csharp_style_prefer_extended_property_pattern` | IDE0170 |
| IDE0019 | PatternMatchingAs | `csharp_style_pattern_matching_over_as_with_null_check` | IDE0019 |
| IDE1005 | ConditionalDelegateCall | `csharp_style_conditional_delegate_call` | IDE1005 |
| IDE0018 | InlinedVariableDeclaration | `csharp_style_inlined_variable_declaration` | IDE0018 |
| IDE0066 | SwitchExpressionPreference | `csharp_style_prefer_switch_expression` | IDE0066 |
| IDE0045 | ConditionalExpressionAssignment | `dotnet_style_prefer_conditional_expression_over_assignment` | IDE0045 |
| IDE0046 | ConditionalExpressionReturn | `dotnet_style_prefer_conditional_expression_over_return` | IDE0046 |
| IDE0039 | LocalOverAnonymousFunction | `csharp_style_prefer_local_over_anonymous_function` | IDE0039 |
| IDE0033 | ExplicitTupleNames | `dotnet_style_explicit_tuple_names` | IDE0033 |
| IDE0037 | InferredTupleNames | `dotnet_style_prefer_inferred_tuple_names` | IDE0037 |
| IDE0340 | UnboundGenericInNameof | `csharp_style_prefer_unbound_generic_type_in_nameof` | IDE0340 |

### Tier 3 — Formatting (standard key)

| CsLint ID | Rule | Standard Key(s) | Standard Rule ID |
|-----------|------|----------------|-----------------|
| CSLINT277 | SortSystemDirectivesFirst | `dotnet_sort_system_directives_first` | (IDE0055) |
| CSLINT278 | SeparateImportDirectiveGroups | `dotnet_separate_import_directive_groups` | (IDE0055) |
| CSLINT279 | NewLineBeforeOpenBrace | `csharp_new_line_before_open_brace` | (IDE0055) |
| CSLINT280 | NewLineBeforeElse | `csharp_new_line_before_else` | (IDE0055) |
| CSLINT281 | NewLineBeforeCatch | `csharp_new_line_before_catch` | (IDE0055) |
| CSLINT282 | NewLineBeforeFinally | `csharp_new_line_before_finally` | (IDE0055) |
| CSLINT283 | NewLineBeforeMembersInObjectInitializers | `csharp_new_line_before_members_in_object_initializers` | (IDE0055) |
| CSLINT284 | NewLineBeforeMembersInAnonymousTypes | `csharp_new_line_before_members_in_anonymous_types` | (IDE0055) |
| CSLINT285 | NewLineBetweenQueryExpressionClauses | `csharp_new_line_between_query_expression_clauses` | (IDE0055) |
| CSLINT286 | SpaceAfterCast | `csharp_space_after_cast` | (IDE0055) |
| CSLINT287 | MethodDeclarationParenSpacing | `csharp_space_between_method_declaration_parameter_list_parentheses`, `_empty_parameter_list_parentheses`, `_name_and_open_parenthesis` | (IDE0055) |
| CSLINT288 | MethodCallParenSpacing | `csharp_space_between_method_call_parameter_list_parentheses`, `_empty_parameter_list_parentheses`, `_name_and_opening_parenthesis` | (IDE0055) |
| CSLINT289 | DotSpacing | `csharp_space_after_dot`, `csharp_space_before_dot` | (IDE0055) |
| CSLINT290 | SquareBracketSpacing | `csharp_space_before_open_square_brackets`, `csharp_space_between_empty_square_brackets`, `csharp_space_between_square_brackets` | (IDE0055) |
| CSLINT291 | DeclarationStatementSpacing | `csharp_space_around_declaration_statements` | (IDE0055) |
| CSLINT292 | Indentation | `csharp_indent_case_contents`, `_switch_labels`, `_labels`, `_block_contents`, `_braces`, `_case_contents_when_block` | (IDE0055) |
| CSLINT293 | PreserveSingleLine | `csharp_preserve_single_line_statements`, `csharp_preserve_single_line_blocks` | (IDE0055) |
| IDE0130 | NamespaceMatchFolder | `dotnet_style_namespace_match_folder` | IDE0130 |
| IDE0200 | MethodGroupConversion | `csharp_style_prefer_method_group_conversion` | IDE0200 |
| IDE0210 | TopLevelStatements | `csharp_style_prefer_top_level_statements` | IDE0210 |

---

## 2. Supported with Custom Name (similar to a standard rule, but uses a non-standard key)

These CsLint rules cover functionality that has a standard .editorconfig key, but CsLint uses its own key name instead.

### Tier 1

| CsLint ID | Rule | CsLint Key | Standard Key (also accepted) | Standard Rule ID | Notes |
|-----------|------|-----------|-------------|-----------------|-------|
| CSLINT008 | MultipleBlankLines | `csharp_no_multiple_blank_lines` | `dotnet_style_allow_multiple_blank_lines_experimental` | IDE2000 | Inverted semantics: CsLint `true` = standard `false`; both accepted |
| CSLINT010 | Utf8FileEncoding | `csharp_store_files_as_utf8` | `charset = utf-8` / `utf-8-bom` | (universal) | Both keys accepted; `charset` enables when value starts with `utf-8` |

### Tier 2 — Naming Rules (legacy)

CsLint has simplified single-key naming rules that are automatically disabled when standard 3-part `dotnet_naming_rule` / `dotnet_naming_symbols` / `dotnet_naming_style` config is present (IDE1006 takes over). These legacy rules use keys that look like standard naming rule names but are parsed as single boolean toggles.

| CsLint ID | Rule | CsLint Key | Standard Equivalent | Notes |
|-----------|------|-----------|-------------------|-------|
| CSLINT100 | TypeNaming | `dotnet_naming_rule.types_should_be_pascal_case` | `dotnet_naming_rule` + `dotnet_naming_symbols` + `dotnet_naming_style` (3-part system) | Disabled when IDE1006 config present |
| CSLINT101 | InterfacePrefix | `dotnet_naming_rule.interface_should_begin_with_i` | (same 3-part system) | Disabled when IDE1006 config present |
| CSLINT102 | MemberNaming | `dotnet_naming_rule.members_should_be_pascal_case` | (same 3-part system) | Disabled when IDE1006 config present |
| CSLINT103 | ParameterLocalNaming | `dotnet_naming_rule.locals_should_be_camel_case` | (same 3-part system) | Disabled when IDE1006 config present |
| CSLINT104 | FieldNaming | `dotnet_naming_rule.private_fields_should_be_underscore_camel_case` | (same 3-part system) | Disabled when IDE1006 config present |
| CSLINT105 | ConstantNaming | `dotnet_naming_rule.constants_should_be_pascal_case` | (same 3-part system) | Disabled when IDE1006 config present |
| CSLINT106 | TypeParameterNaming | `dotnet_naming_rule.type_parameters_should_begin_with_t` | (same 3-part system) | Disabled when IDE1006 config present |

### Tier 3 — Experimental blank line rules (accept both keys)

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
| CSLINT210 | NullChecking | `dotnet_style_null_checking` | `dotnet_style_null_propagation` (IDE0031), `dotnet_style_coalesce_expression` (IDE0029), `dotnet_style_prefer_is_null_check_over_reference_equality_method` (IDE0041) — all accepted | IDE0029/IDE0031/IDE0041 |
| CSLINT230 | BlankLineAfterBlock | `csharp_style_allow_blank_line_after_block` | `dotnet_style_allow_statement_immediately_after_block_experimental` (also accepted) | IDE2003 |

---

## 3. Not Supported — Standard Rules with No CsLint Equivalent

### Would be Tier 3 (style/syntax preferences)

| Standard Key | Standard Rule ID | Description |
|-------------|-----------------|-------------|
| `dotnet_style_readonly_field` | IDE0044 | Add readonly modifier |
| `csharp_style_deconstructed_variable_declaration` | IDE0042 | Deconstruct variable declaration |
| `csharp_style_prefer_readonly_struct` | IDE0250 | Struct can be made readonly |
| `csharp_style_prefer_readonly_struct_member` | IDE0251 | Member can be made readonly |
| `csharp_style_prefer_null_check_over_type_check` | IDE0150 | Prefer null check over type check |
| `csharp_prefer_static_local_function` | IDE0062 | Make local function static |
| `csharp_prefer_static_anonymous_function` | IDE0320 | Make anonymous function static |
| `csharp_prefer_system_threading_lock` | IDE0330 | Prefer System.Threading.Lock |
| `csharp_style_prefer_implicitly_typed_lambda_expression` | IDE0350 | Use implicitly typed lambda |
| `csharp_style_prefer_simple_property_accessors` | IDE0360 | Simplify property accessor |
| `csharp_style_unused_value_expression_statement_preference` | IDE0058 | Remove unused expression value |
| `csharp_style_unused_value_assignment_preference` | IDE0059 | Remove unnecessary value assignment |
| `dotnet_code_quality_unused_parameters` | IDE0060 | Remove unused parameter |
| `dotnet_style_prefer_auto_properties` | IDE0032 | Use auto property |
| `dotnet_style_prefer_foreach_explicit_cast_in_source` | IDE0220 | Add explicit cast in foreach |
| `dotnet_style_parentheses_in_arithmetic_binary_operators` | IDE0047/IDE0048 | Parentheses preferences |
| `dotnet_style_parentheses_in_relational_binary_operators` | IDE0047/IDE0048 | Parentheses preferences |
| `dotnet_style_parentheses_in_other_binary_operators` | IDE0047/IDE0048 | Parentheses preferences |
| `dotnet_style_parentheses_in_other_operators` | IDE0047/IDE0048 | Parentheses preferences |

### Would be Tier 4 (semantic analysis)

These standard rules have **no** CsLint equivalent at all:

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
| Supported with standard key | 64 |
| Supported with custom key (standard equivalent exists) | 16 (9 also accept standard key) |
| Standard rules not supported (style/syntax — would be Tier 3) | 22 |
| Standard rules not supported (semantic — would be Tier 4) | 22 |
