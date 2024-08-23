using QuickyTree.Models;
using QuickyTree.Tabling;

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
            DataQuickSet<IntModel> quickSet = new DataQuickSet<IntModel>();
            quickSet.Add(new IntModel() { Id = 1 });
            quickSet.Add(new IntModel() { Id = 2 });
            quickSet.Add(new IntModel() { Id = 3 });
            quickSet.Add(new IntModel() { Id = 4 });
            quickSet.Add(new IntModel() { Id = 5 });
            quickSet.Add(new IntModel() { Id = 6 });
            quickSet.Add(new IntModel() { Id = 7 });


            var res1 = quickSet.Search(2, 5);
            quickSet.Save();
            var res2 = quickSet.Search(1, 3);


        }
    }
}
