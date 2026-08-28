using QuickyDb.Rb;
using QuickyDb.Tests.RbTests.Models;

namespace QuickyDb.Tests.RbTests;

internal class RbTreeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]

    public void Test1()
    {
        var sets = new RbTable<TestModelA>();

        sets.Add(new TestModelA() { V1 = 3, V2 = "sadas" });
        sets.Add(new TestModelA() { V1 = 2, V2 = "asd" });
        sets.Add(new TestModelA() { V1 = 1, V2 = "asd" });
        sets.Add(new TestModelA() { V1 = 0, V2 = "sadas" });

        var res1 = sets.Search(m => m.V1 > 0).ToArray();
        var res2 = sets.Search(m => m.V2 == "sadas").ToArray();
        var res3 = sets.Search(m => m.V1 < 2 && m.V2 == "asd").ToArray();

        Assert.That(res1.Length == 3 && res2.Length == 2 && res3.Length == 1);
    }
}