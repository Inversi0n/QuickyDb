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
        public QNode Parent { get; set; } //TODO set it


        public QNode(IComparable value)
        {
            Value = value;

        }
    }
}
