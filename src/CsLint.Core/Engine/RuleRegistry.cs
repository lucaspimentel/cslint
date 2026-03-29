using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;
using Cslint.Core.Rules.Tier2;
using Cslint.Core.Rules.Tier3;
#if SEMANTIC
using Cslint.Core.Rules.Tier4;
#endif

namespace Cslint.Core.Engine;

public sealed class RuleRegistry
{
    private readonly List<IRuleDefinition> _rules = [];

    public IReadOnlyList<IRuleDefinition> Rules => _rules;

    public static RuleRegistry CreateDefault()
    {
        var registry = new RuleRegistry();

        // Tier 1: Formatting (text-level)
        registry.Register(new TrailingWhitespaceRule());
        registry.Register(new IndentationRule());
        registry.Register(new LineEndingRule());
        registry.Register(new FinalNewlineRule());
        registry.Register(new MaxLineLengthRule());
        registry.Register(new NoRegionDirectivesRule());
        registry.Register(new FileHeaderRule());
        registry.Register(new MultipleBlankLinesRule());
        registry.Register(new NoBlankLinesAtStartOfFileRule());
        registry.Register(new Utf8FileEncodingRule());

        // Tier 2: Naming conventions (syntax walker)
        registry.Register(new TypeNamingRule());
        registry.Register(new InterfacePrefixRule());
        registry.Register(new MemberNamingRule());
        registry.Register(new LocalVariableNamingRule());
        registry.Register(new ParameterNamingRule());
        registry.Register(new PrivateFieldNamingRule());
        registry.Register(new ReadonlyFieldNamingRule());
        registry.Register(new AccessibleFieldNamingRule());
        registry.Register(new StaticReadonlyFieldNamingRule());
        registry.Register(new ConstantNamingRule());
        registry.Register(new TypeParameterNamingRule());
        registry.Register(new NamingConventionRule());

        // Tier 3: Style preferences (syntax tree analysis)
        registry.Register(new VarPreferenceRule());
        registry.Register(new ExplicitTypePreferenceRule());
        registry.Register(new ExpressionBodiedMethodsRule());
        registry.Register(new ExpressionBodiedPropertiesRule());
        registry.Register(new ExpressionBodiedOperatorsRule());
        registry.Register(new ExpressionBodiedConversionOperatorsRule());
        registry.Register(new ExpressionBodiedIndexersRule());
        registry.Register(new ExpressionBodiedAccessorsRule());
        registry.Register(new BracePreferenceRule());
        registry.Register(new NamespaceDeclarationRule());
        registry.Register(new ThisQualificationRule());
        registry.Register(new ModifierOrderRule());
        registry.Register(new AccessibilityModifierRule());
        registry.Register(new UsingDirectivePlacementRule());
        registry.Register(new PredefinedTypeRule());
        registry.Register(new PatternMatchingRule());
        registry.Register(new NullCoalescingRule());
        registry.Register(new NullPropagationRule());
        registry.Register(new PreferIsNullRule());
        registry.Register(new ThrowExpressionRule());
        registry.Register(new UsingDeclarationRule());
        registry.Register(new TargetTypedNewRule());
        registry.Register(new SimpleDefaultExpressionRule());
        registry.Register(new CompoundAssignmentRule());
        registry.Register(new ObjectInitializerRule());
        registry.Register(new CollectionInitializerRule());
        registry.Register(new ExpressionBodiedLambdasRule());
        registry.Register(new ExpressionBodiedLocalFunctionsRule());
        registry.Register(new StaticLocalFunctionRule());
        registry.Register(new StaticAnonymousFunctionRule());
        registry.Register(new NullCheckOverTypeCheckRule());
        registry.Register(new SystemThreadingLockRule());
        registry.Register(new ImplicitlyTypedLambdaRule());
        registry.Register(new SimplePropertyAccessorsRule());
        registry.Register(new PatternMatchingNotRule());
        registry.Register(new PatternMatchingCombinatorRule());
        registry.Register(new PrimaryConstructorRule());
        registry.Register(new CollectionExpressionRule());
        registry.Register(new TupleSwapRule());
        registry.Register(new Utf8StringLiteralRule());
        registry.Register(new SimplifyInterpolationRule());
        registry.Register(new IndexOperatorRule());
        registry.Register(new RangeOperatorRule());
        registry.Register(new EmbeddedStatementRule());
        registry.Register(new ConsecutiveBracesRule());
        registry.Register(new BlankLineAfterBlockRule());
        registry.Register(new ConstructorInitializerBlankLineRule());
        registry.Register(new ConditionalExpressionBlankLineRule());
        registry.Register(new ArrowExpressionBlankLineRule());
        registry.Register(new InferredMemberNameRule());
        registry.Register(new ExplicitTupleNamesRule());
        registry.Register(new UnboundGenericInNameofRule());
        registry.Register(new SimplifiedBooleanExpressionRule());
        registry.Register(new ExtendedPropertyPatternRule());
        registry.Register(new EmptyFinalizerRule());
        registry.Register(new UnnecessaryInitializationRule());
        registry.Register(new SealedTypePreferenceRule());
        registry.Register(new EmptyCatchBlockRule());
        registry.Register(new NoEmptyStatementsRule());
        registry.Register(new SingleStatementPerLineRule());
        registry.Register(new NoYodaConditionsRule());
        registry.Register(new NoCombinedFieldDeclarationsRule());
        registry.Register(new NoCombinedAttributesRule());
        registry.Register(new AttributesOnOwnLineRule());
        registry.Register(new EnumValuesOnSeparateLinesRule());
        registry.Register(new NoBlankLineAfterOpeningBraceRule());
        registry.Register(new NoBlankLineBeforeClosingBraceRule());
        registry.Register(new NoBlankLineBeforeOpeningBraceRule());
        registry.Register(new ElementsSeparatedByBlankLineRule());
        registry.Register(new FieldsMustBePrivateRule());
        registry.Register(new SingleTypePerFileRule());
        registry.Register(new TrailingCommasRule());
        registry.Register(new KeywordSpacingRule());
        registry.Register(new CommaSpacingRule());
        registry.Register(new SemicolonSpacingRule());
        registry.Register(new OperatorSpacingRule());
        registry.Register(new CommentSpacingRule());
        registry.Register(new OpeningParenthesisSpacingRule());
        registry.Register(new ClosingParenthesisSpacingRule());
        registry.Register(new OpeningBraceSpacingRule());
        registry.Register(new ClosingBraceSpacingRule());
        registry.Register(new ColonSpacingRule());
        registry.Register(new NoMultipleWhitespaceRule());
        registry.Register(new PropertyAccessorOrderRule());
        registry.Register(new EventAccessorOrderRule());
        registry.Register(new ReadonlyFieldOrderRule());
        registry.Register(new ConstantFieldOrderRule());
        registry.Register(new StaticMemberOrderRule());
        registry.Register(new ElementAccessOrderRule());
        registry.Register(new ElementOrderRule());
        registry.Register(new SystemUsingDirectiveOrderRule());
        registry.Register(new AliasUsingDirectiveOrderRule());
        registry.Register(new UsingDirectiveAlphabeticalOrderRule());
        registry.Register(new AliasUsingDirectiveAlphabeticalOrderRule());
        registry.Register(new StaticUsingDirectiveOrderRule());
        registry.Register(new StaticUsingDirectiveAlphabeticalOrderRule());
        registry.Register(new PatternMatchingAsRule());
        registry.Register(new ConditionalDelegateCallRule());
        registry.Register(new InlinedVariableDeclarationRule());
        registry.Register(new SwitchExpressionPreferenceRule());
        registry.Register(new ConditionalExpressionAssignmentRule());
        registry.Register(new ConditionalExpressionReturnRule());
        registry.Register(new LocalOverAnonymousFunctionRule());
        registry.Register(new SortSystemDirectivesFirstRule());
        registry.Register(new SeparateImportDirectiveGroupsRule());
        registry.Register(new MethodGroupConversionRule());
        registry.Register(new TopLevelStatementsRule());
        registry.Register(new NamespaceMatchFolderRule());
        registry.Register(new NewLineBeforeOpenBraceRule());
        registry.Register(new NewLineBeforeElseRule());
        registry.Register(new NewLineBeforeCatchRule());
        registry.Register(new NewLineBeforeFinallyRule());
        registry.Register(new NewLineInObjectInitializerRule());
        registry.Register(new NewLineInAnonymousTypeRule());
        registry.Register(new NewLineInQueryExpressionRule());
        registry.Register(new CastSpacingRule());
        registry.Register(new MethodDeclarationSpacingRule());
        registry.Register(new MethodCallSpacingRule());
        registry.Register(new DotSpacingRule());
        registry.Register(new SquareBracketSpacingRule());
        registry.Register(new DeclarationStatementSpacingRule());
        registry.Register(new IndentationFormattingRule());
        registry.Register(new PreserveSingleLineRule());
        registry.Register(new RethrowRule());
        registry.Register(new PropertySelfAssignmentInSetterRule());
        registry.Register(new ThrowInFinallyRule());
        registry.Register(new StackallocInLoopRule());
        registry.Register(new ProtectedMemberInSealedTypeRule());
        registry.Register(new PublicConstructorOnAbstractTypeRule());
        registry.Register(new StaticHolderShouldBeSealedRule());
        registry.Register(new VirtualEventRule());
        registry.Register(new ThreadStaticOnNonStaticFieldRule());
        registry.Register(new ThreadStaticWithInitializerRule());
        registry.Register(new ObsoleteWithoutMessageRule());
        registry.Register(new EmptyInterfaceRule());
        registry.Register(new TypeOutsideNamespaceRule());
        registry.Register(new VisibleInstanceFieldRule());
        registry.Register(new WriteOnlyPropertyRule());
        registry.Register(new VisibleNonConstStaticFieldRule());
        registry.Register(new VisibleNestedTypeRule());
        registry.Register(new EnumStorageShouldBeInt32Rule());
        registry.Register(new EnumValuesPrefixedWithTypeNameRule());
        registry.Register(new FlagsEnumValuesRule());
        registry.Register(new ZeroLengthArrayRule());
        registry.Register(new ConstantArrayAsArgumentRule());
        registry.Register(new NumericPlaceholderRule());
        registry.Register(new IdentifierUnderscoreRule());
        registry.Register(new FlagsEnumPluralNameRule());
        registry.Register(new IdentifierMatchesKeywordRule());
        registry.Register(new IdentifierContainsTypeNameRule());
        registry.Register(new PropertyNameMatchesGetMethodRule());
        registry.Register(new PascalCasePlaceholderRule());
        registry.Register(new AvoidOutParametersRule());
        registry.Register(new CatchGeneralExceptionRule());
        registry.Register(new DuplicateIndexedElementInitRule());
        registry.Register(new PropertySelfAssignmentRule());
        registry.Register(new UnusedParameterRule());
        registry.Register(new AutoPropertyPreferenceRule());
        registry.Register(new DeconstructedVariableRule());
        registry.Register(new ReadonlyFieldPreferenceRule());
        registry.Register(new ReadonlyStructPreferenceRule());
        registry.Register(new ReadonlyStructMemberPreferenceRule());
        registry.Register(new RemoveUnnecessaryParenthesesRule());
        registry.Register(new AddParenthesesForClarityRule());

#if SEMANTIC
        // Tier 4: Semantic analysis
        registry.Register(new UnusedUsingRule());
        registry.Register(new UnusedLocalVariableRule());
        registry.Register(new UnreachableCodeRule());
        registry.Register(new DuplicateEnumValueRule());
        registry.Register(new SelfAssignmentRule());
        registry.Register(new UnnecessaryCastRule());
        registry.Register(new RedundantAwaitRule());
        registry.Register(new UnusedPrivateMemberRule());
        registry.Register(new UnreadPrivateMemberRule());
        registry.Register(new UnusedValueExpressionStatementRule());
        registry.Register(new UnusedValueAssignmentRule());
        registry.Register(new PreferForeachExplicitCastRule());
#endif

        return registry;
    }

    public static IReadOnlyDictionary<string, List<string>> GetAliases() =>
        PragmaAliasMap.GetAliasesByCslintId();

    public void Register(IRuleDefinition rule) => _rules.Add(rule);
}
