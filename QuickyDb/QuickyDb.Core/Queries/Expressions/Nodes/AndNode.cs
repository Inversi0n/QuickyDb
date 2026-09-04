namespace QuickyDb.Core.Queries.Expressions.Nodes;

public sealed class AndNode : QueryNode
{
    public QueryNode Left { get; }
    public QueryNode Right { get; }
    public AndNode(QueryNode left, QueryNode right) { Left = left; Right = right; }
}
