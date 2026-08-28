namespace QuickyDb.Core.Queries.Expressions.Nodes
{
    public sealed class NotNode : QueryNode
    {
        public QueryNode Inner { get; }
        public NotNode(QueryNode inner) { Inner = inner; }
    }
}
