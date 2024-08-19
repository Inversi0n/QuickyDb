namespace QuickyTree
{
    public class Tree<TIndex> where TIndex : IComparable<TIndex>
    {
        private readonly string _fileName;

        public TreeNode<TIndex> Root { get; set; }

        public void Add(TIndex item)
        {
            if (Root == null)
            {
                Root = new TreeNode<TIndex>(item);
                return;
            }

            var curNode = Root;
            while (true)
            {
                if (curNode.Value.CompareTo(item) > 0)
                {
                    if (curNode.LeftNode == null)
                    {
                        curNode.LeftNode = new TreeNode<TIndex>(item);
                        break;
                    }
                    curNode = curNode.LeftNode;
                }
                else
                {
                    if (curNode.RightNode == null)
                    {
                        curNode.RightNode = new TreeNode<TIndex>(item);
                        break;
                    }
                    curNode = curNode.RightNode;
                }
            }

        }

        private TreeNode<TIndex> FindExisted(TIndex item)
        {
            var curNode = Root;
            while (true)
            {
                var compareRes = curNode.Value.CompareTo(item);
                if (compareRes == 0)
                    return curNode;

                if (compareRes > 0)
                {
                    if (curNode.LeftNode == null)
                    {
                        throw new ApplicationException($"Unable to find {item} element");
                    }
                    curNode = curNode.LeftNode;
                }
                else
                {
                    if (curNode.RightNode == null)
                    {
                        throw new ApplicationException($"Unable to find {item} element");
                    }
                    curNode = curNode.RightNode;
                }
            }
        }
        public void Update(TIndex item)
        {

        }
        public void Remove(TIndex item) { }
        public void Delete(TIndex item)
        {

        }

    }
}
