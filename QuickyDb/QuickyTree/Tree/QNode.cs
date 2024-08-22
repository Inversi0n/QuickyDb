using QuickyTree.FileUtils.Models;
using QuickyTree.Interfaces;

namespace QuickyTree.Tree
{
    public class QNode
    {
        public IComparable Value { get; set; }
        public ModelUnitMetadata StoringData { get; set; }
        public QNode LeftNode { get; set; }
        public QNode RightNode { get; set; }
        public QNode Parent { get; set; }


        public QNode(IComparable value, QNode parent, ModelUnitMetadata storingData)
        {
            Value = value;
            Parent = parent;
            StoringData = storingData;
        }
    }
}
