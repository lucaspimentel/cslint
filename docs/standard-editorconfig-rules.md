# Standard C# .editorconfig Rules

## 1. Universal EditorConfig Properties (all IDEs)

These are part of the [EditorConfig spec](https://editorconfig.org/) itself:

- `indent_style` — `tab` / `space`
- `indent_size` — integer or `tab`
- `tab_width` — integer
- `end_of_line` — `lf` / `crlf` / `cr`
- `charset` — `utf-8`, `utf-8-bom`, `utf-16be`, `utf-16le`, `latin1`
- `trim_trailing_whitespace` — `true` / `false`
- `insert_final_newline` — `true` / `false`
- `max_line_length` — integer or `off`

---

## 2. .NET Language/Code Style Rules (Visual Studio, Rider, `dotnet format`)

These use `value:severity` syntax (e.g., `true:warning`).

### `this.` qualification (IDE0003/IDE0009)

- `dotnet_style_qualification_for_field`
- `dotnet_style_qualification_for_property`
- `dotnet_style_qualification_for_method`
- `dotnet_style_qualification_for_event`

### Type keywords vs framework names (IDE0049)

- `dotnet_style_predefined_type_for_locals_parameters_members`
- `dotnet_style_predefined_type_for_member_access`

### Modifiers (IDE0036, IDE0040, IDE0044)

- `csharp_preferred_modifier_order`
- `dotnet_style_require_accessibility_modifiers`
- `dotnet_style_readonly_field`

### Parentheses (IDE0047/IDE0048)

- `dotnet_style_parentheses_in_arithmetic_binary_operators`
- `dotnet_style_parentheses_in_relational_binary_operators`
- `dotnet_style_parentheses_in_other_binary_operators`
- `dotnet_style_parentheses_in_other_operators`

### Expression-level preferences

- `dotnet_style_object_initializer` (IDE0017)
- `dotnet_style_collection_initializer` (IDE0028)
- `dotnet_style_prefer_collection_expression` (IDE0028/IDE0300-IDE0306)
- `dotnet_style_explicit_tuple_names` (IDE0033)
- `dotnet_style_prefer_inferred_tuple_names` (IDE0037)
- `dotnet_style_prefer_inferred_anonymous_type_member_names` (IDE0037)
- `dotnet_style_prefer_auto_properties` (IDE0032)
- `dotnet_style_prefer_conditional_expression_over_assignment` (IDE0045)
- `dotnet_style_prefer_conditional_expression_over_return` (IDE0046)
- `dotnet_style_prefer_compound_assignment` (IDE0054/IDE0074)
- `dotnet_style_prefer_simplified_interpolation` (IDE0071)
- `dotnet_style_prefer_simplified_boolean_expressions` (IDE0075)
- `dotnet_style_prefer_foreach_explicit_cast_in_source` (IDE0220)
- `dotnet_style_namespace_match_folder` (IDE0130)

### Null-checking preferences

- `dotnet_style_coalesce_expression` (IDE0029/IDE0030/IDE0270)
- `dotnet_style_null_propagation` (IDE0031)
- `dotnet_style_prefer_is_null_check_over_reference_equality_method` (IDE0041)

### `var` usage (IDE0007/IDE0008)

- `csharp_style_var_for_built_in_types`
- `csharp_style_var_when_type_is_apparent`
- `csharp_style_var_elsewhere`

### Expression-bodied members (IDE0021-IDE0027, IDE0053, IDE0061)

- `csharp_style_expression_bodied_constructors`
- `csharp_style_expression_bodied_methods`
- `csharp_style_expression_bodied_operators`
- `csharp_style_expression_bodied_properties`
- `csharp_style_expression_bodied_indexers`
- `csharp_style_expression_bodied_accessors`
- `csharp_style_expression_bodied_lambdas`
- `csharp_style_expression_bodied_local_functions`

### Pattern matching (IDE0019, IDE0020, IDE0066, IDE0078, IDE0083, IDE0260)

- `csharp_style_pattern_matching_over_as_with_null_check`
- `csharp_style_pattern_matching_over_is_with_cast_check`
- `csharp_style_prefer_switch_expression`
- `csharp_style_prefer_pattern_matching`
- `csharp_style_prefer_not_pattern`
- `csharp_style_prefer_extended_property_pattern` (IDE0170)

### Code block preferences

- `csharp_prefer_braces` (IDE0011)
- `csharp_prefer_simple_using_statement` (IDE0063)
- `csharp_style_namespace_declarations` (IDE0160/IDE0161) — `file_scoped` or `block_scoped`
- `csharp_style_prefer_top_level_statements` (IDE0210/IDE0211)
- `csharp_style_prefer_primary_constructors` (IDE0290)

### Other C# style preferences

- `csharp_style_throw_expression` (IDE0016)
- `csharp_style_inlined_variable_declaration` (IDE0018)
- `csharp_prefer_simple_default_expression` (IDE0034)
- `csharp_style_deconstructed_variable_declaration` (IDE0042)
- `csharp_style_prefer_local_over_anonymous_function` (IDE0039)
- `csharp_style_conditional_delegate_call` (IDE1005)
- `csharp_style_prefer_index_operator` (IDE0056)
- `csharp_style_prefer_range_operator` (IDE0057)
- `csharp_style_implicit_object_creation_when_type_is_apparent` (IDE0090)
- `csharp_style_prefer_tuple_swap` (IDE0180)
- `csharp_style_prefer_method_group_conversion` (IDE0200)
- `csharp_style_prefer_utf8_string_literals` (IDE0230)
- `csharp_style_prefer_readonly_struct` (IDE0250)
- `csharp_style_prefer_readonly_struct_member` (IDE0251)
- `csharp_style_prefer_null_check_over_type_check` (IDE0150)
- `csharp_prefer_static_local_function` (IDE0062)
- `csharp_prefer_static_anonymous_function` (IDE0320)
- `csharp_prefer_system_threading_lock` (IDE0330)
- `csharp_style_prefer_unbound_generic_type_in_nameof` (IDE0340)
- `csharp_style_prefer_implicitly_typed_lambda_expression` (IDE0350)
- `csharp_style_prefer_simple_property_accessors` (IDE0360)
- `csharp_using_directive_placement` (IDE0065)
- `csharp_style_unused_value_expression_statement_preference` (IDE0058)
- `csharp_style_unused_value_assignment_preference` (IDE0059)
- `dotnet_code_quality_unused_parameters` (IDE0060)

### File header (IDE0073)

- `file_header_template`

### Experimental blank line rules (IDE2000-IDE2006)

- `dotnet_style_allow_multiple_blank_lines_experimental`
- `csharp_style_allow_embedded_statements_on_same_line_experimental`
- `csharp_style_allow_blank_lines_between_consecutive_braces_experimental`
- `dotnet_style_allow_statement_immediately_after_block_experimental`
- `csharp_style_allow_blank_line_after_colon_in_constructor_initializer_experimental`
- `csharp_style_allow_blank_line_after_token_in_conditional_expression_experimental`
- `csharp_style_allow_blank_line_after_token_in_arrow_expression_clause_experimental`

---

## 3. C# Formatting Rules (IDE0055) — Visual Studio, Rider, `dotnet format`

### New-line options

- `csharp_new_line_before_open_brace` — values: `all`, `none`, or comma-separated list of: `accessors`, `anonymous_methods`, `anonymous_types`, `control_blocks`, `events`, `indexers`, `lambdas`, `local_functions`, `methods`, `object_collection_array_initializers`, `properties`, `types`
- `csharp_new_line_before_else` — `true` / `false`
- `csharp_new_line_before_catch` — `true` / `false`
- `csharp_new_line_before_finally` — `true` / `false`
- `csharp_new_line_before_members_in_object_initializers` — `true` / `false`
- `csharp_new_line_before_members_in_anonymous_types` — `true` / `false`
- `csharp_new_line_between_query_expression_clauses` — `true` / `false`

### Indentation options

- `csharp_indent_case_contents` — `true` / `false`
- `csharp_indent_switch_labels` — `true` / `false`
- `csharp_indent_labels` — `flush_left`, `one_less_than_current`, `no_change`
- `csharp_indent_block_contents` — `true` / `false`
- `csharp_indent_braces` — `true` / `false`
- `csharp_indent_case_contents_when_block` — `true` / `false`

### Spacing options

- `csharp_space_after_cast` — `true` / `false`
- `csharp_space_after_keywords_in_control_flow_statements` — `true` / `false`
- `csharp_space_between_parentheses` — `control_flow_statements`, `expressions`, `type_casts`, `false`
- `csharp_space_before_colon_in_inheritance_clause` — `true` / `false`
- `csharp_space_after_colon_in_inheritance_clause` — `true` / `false`
- `csharp_space_around_binary_operators` — `before_and_after`, `none`, `ignore`
- `csharp_space_between_method_declaration_parameter_list_parentheses` — `true` / `false`
- `csharp_space_between_method_declaration_empty_parameter_list_parentheses` — `true` / `false`
- `csharp_space_between_method_declaration_name_and_open_parenthesis` — `true` / `false`
- `csharp_space_between_method_call_parameter_list_parentheses` — `true` / `false`
- `csharp_space_between_method_call_empty_parameter_list_parentheses` — `true` / `false`
- `csharp_space_between_method_call_name_and_opening_parenthesis` — `true` / `false`
- `csharp_space_after_comma` — `true` / `false`
- `csharp_space_before_comma` — `true` / `false`
- `csharp_space_after_dot` — `true` / `false`
- `csharp_space_before_dot` — `true` / `false`
- `csharp_space_after_semicolon_in_for_statement` — `true` / `false`
- `csharp_space_before_semicolon_in_for_statement` — `true` / `false`
- `csharp_space_around_declaration_statements` — `ignore` / `false`
- `csharp_space_before_open_square_brackets` — `true` / `false`
- `csharp_space_between_empty_square_brackets` — `true` / `false`
- `csharp_space_between_square_brackets` — `true` / `false`

### Wrap options

- `csharp_preserve_single_line_statements` — `true` / `false`
- `csharp_preserve_single_line_blocks` — `true` / `false`

### .NET formatting (shared C#/VB)

- `dotnet_sort_system_directives_first` — `true` / `false`
- `dotnet_separate_import_directive_groups` — `true` / `false`

---

## 4. Naming Rules (IDE1006) — Visual Studio, Rider

Three-part system using `dotnet_naming_rule`, `dotnet_naming_symbols`, `dotnet_naming_style`.

### Syntax

```ini
# Define a symbol group
dotnet_naming_symbols.<group_name>.applicable_kinds = <kinds>
dotnet_naming_symbols.<group_name>.applicable_accessibilities = <accessibilities>
dotnet_naming_symbols.<group_name>.required_modifiers = <modifiers>

# Define a naming style
dotnet_naming_style.<style_name>.capitalization = <capitalization>
dotnet_naming_style.<style_name>.required_prefix = <prefix>
dotnet_naming_style.<style_name>.required_suffix = <suffix>
dotnet_naming_style.<style_name>.word_separator = <separator>

# Define a naming rule
dotnet_naming_rule.<rule_name>.symbols = <group_name>
dotnet_naming_rule.<rule_name>.style = <style_name>
dotnet_naming_rule.<rule_name>.severity = <severity>
```

### Symbol kinds (`applicable_kinds`)

`*`, `namespace`, `class`, `struct`, `interface`, `enum`, `property`, `method`, `field`, `event`, `delegate`, `parameter`, `type_parameter`, `local`, `local_function`

### Accessibilities (`applicable_accessibilities`)

`*`, `public`, `internal` (or `friend`), `private`, `protected`, `protected_internal` (or `protected_friend`), `private_protected`, `local`

### Required modifiers (`required_modifiers`)

`abstract` (or `must_inherit`), `async`, `const`, `readonly`, `static` (or `shared`)

### Capitalization styles

`pascal_case`, `camel_case`, `first_word_upper`, `all_upper`, `all_lower`

### Example

```ini
[*.{cs,vb}]

# Private fields must start with _ and be camelCase
dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.underscore_camel_case.capitalization = camel_case
dotnet_naming_style.underscore_camel_case.required_prefix = _

dotnet_naming_rule.private_fields_underscore.symbols = private_fields
dotnet_naming_rule.private_fields_underscore.style = underscore_camel_case
dotnet_naming_rule.private_fields_underscore.severity = warning

# Interfaces must start with I
dotnet_naming_symbols.interfaces.applicable_kinds = interface
dotnet_naming_symbols.interfaces.applicable_accessibilities = *

dotnet_naming_style.begins_with_i.capitalization = pascal_case
dotnet_naming_style.begins_with_i.required_prefix = I

dotnet_naming_rule.interfaces_begin_with_i.symbols = interfaces
dotnet_naming_rule.interfaces_begin_with_i.style = begins_with_i
dotnet_naming_rule.interfaces_begin_with_i.severity = warning
```

---

## 5. Severity Configuration

### Per-option (inline with value)

```ini
csharp_style_var_elsewhere = true:warning
```

### Per-rule (by diagnostic ID)

```ini
dotnet_diagnostic.IDE0007.severity = warning
```

### By category

```ini
dotnet_analyzer_diagnostics.category-Style.severity = suggestion
```

### Severity levels

- `none` — do not show
- `silent` (or `refactoring`) — show as refactoring only
- `suggestion` — show as suggestion (dots in editor)
- `warning` — show as warning
- `error` — show as error

---

## Sources

- [Code-style rules overview - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/)
- [C# formatting options - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/csharp-formatting-options)
- [.NET formatting options - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/dotnet-formatting-options)
- [Code-style naming rules - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/naming-rules)
- [.NET code style rule options - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options)
