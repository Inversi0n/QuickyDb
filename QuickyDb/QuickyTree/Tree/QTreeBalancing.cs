using System.Xml.Linq;

namespace QuickyTree.Tree
{
    public partial class QTree
    {
        public void Balance()
        {
            var newRoot = buildTree(Root);
        }
        /* This function traverse the skewed binary tree and 
       stores its nodes pointers in vector nodes[] */
        public virtual void storeBSTNodes(QNode root, List<QNode> nodes)
        {
            // Base case 
            if (root == null)
            {
                return;
            }

            // Store nodes in Inorder (which is sorted 
            // order for BST) 
            storeBSTNodes(root.LeftNode, nodes);
            nodes.Add(root);
            storeBSTNodes(root.RightNode, nodes);
        }

        /* Recursive function to construct binary tree */
        public virtual QNode buildTreeUtil(List<QNode> nodes, int start, int end)
        {
            // base case 
            if (start > end)
            {
                return null;
            }

            /* Get the middle element and make it root */
            int mid = (start + end) / 2;
            QNode node = nodes[mid];

            /* Using index in Inorder traversal, construct 
               left and right subtress */
            node.LeftNode = buildTreeUtil(nodes, start, mid - 1);
            if (node.LeftNode != null)
                node.LeftNode.Parent = node;

            node.RightNode = buildTreeUtil(nodes, mid + 1, end);
            if (node.RightNode != null)
                node.RightNode.Parent = node;

            return node;
        }

        // This functions converts an unbalanced BST to 
        // a balanced BST 
        public virtual QNode buildTree(QNode root)
        {
            // Store nodes of given BST in sorted order 
            List<QNode> nodes = new List<QNode>();
            storeBSTNodes(root, nodes);

            // Constructs BST from nodes[] 
            int n = nodes.Count;
            Root = buildTreeUtil(nodes, 0, n - 1);
            return root;
        }

        public virtual void preOrder(QNode node)
        {
            if (node == null)
            {
                return;
            }

            Console.Write(node.Value.ToString() + " ");
            preOrder(node.LeftNode);
            preOrder(node.RightNode);
        }
    }
}
