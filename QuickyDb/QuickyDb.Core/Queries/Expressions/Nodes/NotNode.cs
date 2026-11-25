namespace QuickyDb.Core.Queries.Expressions.Nodes;

/// <summary>
/// Logical NOT of a node: a model matches if it does not match the inner node.
/// </summary>
public sealed class NotNode : QueryNodeBase
{
    /// <summary>Negated node.</summary>
    public QueryNodeBase Inner { get; }

    /// <summary>
    /// Creates a node that negates another node.
    /// </summary>
    /// <param name="inner">Node to negate.</param>
    public NotNode(QueryNodeBase inner)
    {
        Inner = inner;
    }
}
