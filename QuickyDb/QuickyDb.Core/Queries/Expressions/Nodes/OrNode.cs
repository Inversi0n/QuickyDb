namespace QuickyDb.Core.Queries.Expressions.Nodes;

/// <summary>
/// Logical OR of two nodes: a model matches if it matches at least one of them.
/// </summary>
public sealed class OrNode : QueryNodeBase
{
    /// <summary>Left operand.</summary>
    public QueryNodeBase Left { get; }

    /// <summary>Right operand.</summary>
    public QueryNodeBase Right { get; }

    /// <summary>
    /// Creates a node from two operands.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    public OrNode(QueryNodeBase left, QueryNodeBase right)
    {
        Left = left;
        Right = right;
    }
}
