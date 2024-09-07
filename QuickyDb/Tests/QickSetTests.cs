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
        public void ExpressionParsing()
        {
            DataQuickSet<TestModel> quickSet = new DataQuickSet<TestModel>();

            var val = 5; 
            var qs = quickSet.Search(a => Math.Pow(val, 2) >= a.Id+1);
        }

        //TODO Make file manager for IFileWrappers
        //TODO Make parsing expressions: > < contains
        [Test]
        public void Test1()
        {
            DataQuickSet<TestModel> quickSet = new DataQuickSet<TestModel>();

            var val = 5;
            quickSet.Add(new TestModel(1));
            quickSet.Add(new TestModel(2));
            var added = quickSet.Search(2);
            quickSet.Add(new TestModel(3));
            added = quickSet.Search(3);
            quickSet.Add(new TestModel(4));
            added = quickSet.Search(4);
            quickSet.Add(new TestModel(5));
            added = quickSet.Search(5);
            quickSet.Add(new TestModel(6));
            quickSet.Add(new TestModel(7));


            //var res1 = quickSet.Search(2, 5);
            quickSet.Save();
            var res2 = quickSet.Search(1, 2);

            var a = 0;

        }
        public class TestModel : IntModel
        {
            public string Text { get; set; } = "Hello";
            public TestModel(int id)
            {
                Id = id; ;
            }
            public TestModel()
            {

            }
        }
    }
}
