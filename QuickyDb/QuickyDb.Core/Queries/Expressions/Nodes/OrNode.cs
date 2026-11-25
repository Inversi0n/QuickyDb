namespace QuickyDb.Core.Queries.Expressions.Nodes;

/// <summary>
/// Logical OR of two nodes: a model matches if it matches at least one of them.
/// </summary>
public sealed class OrNode : QueryNode
{
    /// <summary>Left operand.</summary>
    public QueryNode Left { get; }

    /// <summary>Right operand.</summary>
    public QueryNode Right { get; }

    /// <summary>
    /// Creates a node from two operands.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    public OrNode(QueryNode left, QueryNode right)
    {
        Left = left;
        Right = right;
    }
}
