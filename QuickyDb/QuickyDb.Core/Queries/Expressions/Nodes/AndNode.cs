namespace QuickyDb.Core.Queries.Expressions.Nodes;

/// <summary>
/// Logical AND of two nodes: a model matches if it matches both of them.
/// </summary>
public sealed class AndNode : QueryNode
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
    public AndNode(QueryNode left, QueryNode right)
    {
        Left = left;
        Right = right;
    }
}
