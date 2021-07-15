using CodeSmith.Data.Rules;
using CodeSmith.Data.Rules.Validation;

namespace CodeSmith.Data.Attributes
{
    public class NotNullAttribute : RuleAttributeBase
    {
        public NotNullAttribute()
            : this(EntityState.Dirty)
        {
        }

        public NotNullAttribute(EntityState state)
        {
            State = state;
        }

        public override IRule CreateRule(string property)
        {
            return new NotNullRule(property);
        }

        public override bool IsValid(object value)
        {
            return value != null;
        }
    }
}
