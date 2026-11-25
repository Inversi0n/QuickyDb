using System.Linq.Expressions;
using System.Reflection;

namespace QuickyDb.Core.Queries.Expressions.Nodes;

/// <summary>
/// Comparison of a model property with a value.
/// </summary>
public sealed class ConditionNode : QueryNode
{
    /// <summary>Property of the model that is compared.</summary>
    public PropertyInfo Property { get; }

    /// <summary>Value the property is compared with. It is evaluated when the query is parsed.</summary>
    public object Value { get; }

    /// <summary>
    /// <para>Comparison operator: Equal, NotEqual, LessThan, LessThanOrEqual, GreaterThan or GreaterThanOrEqual.</para>
    /// <para>The property is always the left operand: if the query had it on the right, the operator is flipped.</para>
    /// </summary>
    public ExpressionType Operator { get; }

    /// <summary>
    /// Creates a comparison.
    /// </summary>
    /// <param name="property">Property of the model that is compared.</param>
    /// <param name="op">Comparison operator, with the property as the left operand.</param>
    /// <param name="value">Value the property is compared with.</param>
    public ConditionNode(PropertyInfo property, ExpressionType op, object value)
    {
        Property = property;
        Operator = op;
        Value = value;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Property.Name} {Operator} {Value}";
}
