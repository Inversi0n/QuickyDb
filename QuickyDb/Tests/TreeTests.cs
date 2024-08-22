using QuickyTree.Tree;

namespace Tests
{
    public class TreeTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var tree = new QTree();
            tree.Add(5, null);
            tree.Add(7, null);
            tree.Add(3, null);
            tree.Add(2, null);
            tree.Add(1, null);
            tree.Add(8, null);
            tree.Add(9, null);
            tree.Add(10, null);

            var founded = tree.Search(3);
            tree.preOrder(tree.Root);

            tree.buildTree(tree.Root);

            tree.preOrder(tree.Root);
            founded = tree.Search(3);

            Assert.Pass();
        }
    }
}