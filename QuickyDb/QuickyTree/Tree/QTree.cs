using QuickyTree.FileUtils.Models;
using QuickyTree.Interfaces;

namespace QuickyTree.Tree
{
    public partial class QTree
    {
        public QNode Root { get; set; }

        public QNode Add(IComparable item, ModelUnitMetadata storingData)
        {
            if (Root == null)
            {
                Root = new QNode(item, null, storingData);
                return Root;
            }

            var curNode = Root;
            while (true)
            {
                if (curNode.Value.CompareTo(item) > 0)
                {
                    if (curNode.LeftNode == null)
                    {
                        curNode.LeftNode = new QNode(item, curNode, storingData);
                        return curNode.LeftNode;
                    }
                    curNode = curNode.LeftNode;
                }
                else
                {
                    if (curNode.RightNode == null)
                    {
                        curNode.RightNode = new QNode(item, curNode, storingData);
                        return curNode.RightNode;
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

            var result = new QNode[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var foundNode = FindNode(curNode, item);
                result[i] = foundNode;
                if (i == items.Length - 1)
                    continue;

                var nextItem = items[i];
                var top = DownFindNode(foundNode, nextItem);
            }
            return result;
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

     
        public void Update(IComparable item)
        {

        }
        public void Remove(IComparable item) { }
        public void Delete(IComparable item)
        {

        }

    }
}
