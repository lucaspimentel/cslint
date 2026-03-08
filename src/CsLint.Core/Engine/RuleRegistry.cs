using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;
using Cslint.Core.Rules.Tier2;
using Cslint.Core.Rules.Tier3;

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

        // Tier 2: Naming conventions (syntax walker)
        registry.Register(new TypeNamingRule());
        registry.Register(new InterfacePrefixRule());
        registry.Register(new MemberNamingRule());
        registry.Register(new ParameterLocalNamingRule());
        registry.Register(new FieldNamingRule());
        registry.Register(new ConstantNamingRule());

        // Tier 3: Style preferences (syntax tree analysis)
        registry.Register(new VarPreferenceRule());
        registry.Register(new ExpressionBodiedRule());
        registry.Register(new BracePreferenceRule());
        registry.Register(new NamespaceDeclarationRule());
        registry.Register(new ThisQualificationRule());
        registry.Register(new ModifierOrderRule());
        registry.Register(new AccessibilityModifierRule());
        registry.Register(new UsingDirectivePlacementRule());
        registry.Register(new PredefinedTypeRule());
        registry.Register(new PatternMatchingRule());
        registry.Register(new NullCheckingRule());
        registry.Register(new UsingDeclarationRule());
        registry.Register(new TargetTypedNewRule());
        registry.Register(new SimpleDefaultExpressionRule());
        registry.Register(new CompoundAssignmentRule());
        registry.Register(new ObjectInitializerRule());
        registry.Register(new CollectionInitializerRule());
        registry.Register(new ExpressionBodiedLambdasRule());
        registry.Register(new ExpressionBodiedLocalFunctionsRule());
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

        return registry;
    }

    public void Register(IRuleDefinition rule) => _rules.Add(rule);
}
