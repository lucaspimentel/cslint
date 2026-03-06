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

        return registry;
    }

    public void Register(IRuleDefinition rule) => _rules.Add(rule);
}
