using QuickyTree;

namespace Tests
{
    internal class QickSetTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
           DataQuickSet<int> quickSet = new DataQuickSet<int>();
            quickSet.Add(1);
            quickSet.Add(2);
            quickSet.Add(3);
            quickSet.Add(4);
            quickSet.Add(5);
            quickSet.Add(6);
            quickSet.Add(7);


            var res1 = quickSet.Search(2, 5);
            quickSet.Save();
            var res2 = quickSet.Search(1, 3);
                   

        }
    }
}
