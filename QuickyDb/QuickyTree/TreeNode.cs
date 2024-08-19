namespace QuickyTree
{
    public class TreeNode<TIndex> where TIndex : IComparable<TIndex>
    {
        public TIndex Value { get; set; }
        public long Position { get; set; }
        public long Length { get; set; }
        public TreeNode<TIndex> LeftNode { get; set; }
        public TreeNode<TIndex> RightNode { get; set; }

        public TreeNode(TIndex value)
        {
            Value = value;
        }
    }
}
