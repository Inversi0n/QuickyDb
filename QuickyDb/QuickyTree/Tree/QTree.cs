using QuickyTree.Interfaces;

namespace QuickyTree.Tree
{
    public class QTree
    {
        public QNode Root { get; set; }

        public void Add(IComparable item)
        {
            if (Root == null)
            {
                Root = new QNode(item);
                return;
            }

            var curNode = Root;
            while (true)
            {
                if (curNode.Value.CompareTo(item) > 0)
                {
                    if (curNode.LeftNode == null)
                    {
                        curNode.LeftNode = new QNode(item);
                        break;
                    }
                    curNode = curNode.LeftNode;
                }
                else
                {
                    if (curNode.RightNode == null)
                    {
                        curNode.RightNode = new QNode(item);
                        break;
                    }
                    curNode = curNode.RightNode;
                }
            }

        }

        public QNode Search(IComparable item)
        {
            return FindNode(Root, item);
        }
        public QNode[] SearchAll(IComparable[] items)
        {
            var curNode = Root;


            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var foundNode = FindNode(curNode, item);
                if (i == items.Length - 1)
                    continue;

                var nextItem = items[i];
                var top = DownFindNode(foundNode, nextItem);


            }
        }
        private QNode DownFindNode(QNode curNode, IComparable item)
        {
            while (true)
            {
                var compareRes = curNode.Value.CompareTo(item);
                if (compareRes == 0)
                    return curNode;


                var parent = curNode.Parent;
                if (parent == null)
                    throw new Exception("No papa :(");

                compareRes = parent.Value.CompareTo(item);
                if (compareRes == 0)
                    return curNode;

                var leftCompare = parent.LeftNode.Value.CompareTo(item);
                var rightCompare = parent.RightNode.Value.CompareTo(item);
                if (leftCompare == -1
                    && rightCompare == 1)
                    return parent;

                curNode = parent;
            }
        }
        private static QNode FindNode(QNode curNode, IComparable item)
        {
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

        public void Balance()
        {

        }
        public void Update(IComparable item)
        {

        }
        public void Remove(IComparable item) { }
        public void Delete(IComparable item)
        {

        }

    }
}
