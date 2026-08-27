using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDb.Core.Queries.Expressions.Nodes
{
    public sealed class ConditionNode : QueryNode
    {
        public PropertyInfo Property { get; }
        public ExpressionType Operator { get; }
        public object Value { get; }

        public ConditionNode(PropertyInfo property, ExpressionType op, object value)
        { 
            Property = property;
            Operator = op;
            Value = value;
        }

        public override string ToString() => $"{Property.Name} {Operator} {Value}";
    }
}
