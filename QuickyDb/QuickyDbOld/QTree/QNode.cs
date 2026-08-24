using QuickyTree.FileUtils.Models;
using System;

namespace QuickyTree.Tree
{
    public class QNode
    {
        public IComparable Value { get; set; }
        public SavedLocationMetadata StoringData { get; set; }
        public QNode LeftNode { get; set; }
        public QNode RightNode { get; set; }
        public QNode Parent { get; set; }


        public QNode(IComparable value, QNode parent, SavedLocationMetadata storingData)
        {
            Value = value;
            Parent = parent;
            StoringData = storingData;
        }
    }
}
