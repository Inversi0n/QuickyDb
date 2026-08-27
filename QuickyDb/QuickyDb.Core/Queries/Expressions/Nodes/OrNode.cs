namespace QuickyDb.Core.Queries.Expressions.Nodes
{
    public sealed class OrNode : QueryNode
    {
        public QueryNode Left { get; }
        public QueryNode Right { get; }
        public OrNode(QueryNode left, QueryNode right) { Left = left; Right = right; }
    }
}
