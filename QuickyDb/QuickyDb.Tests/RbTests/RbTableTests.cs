using QuickyDb.Rb;
using QuickyDb.Tests.RbTests.Models;

namespace QuickyDb.Tests.RbTests;

[TestFixture]
internal class RbTableTests
{
    // Basic generatl test
    [Test]
    public void Search_ModelsByConditions()
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

    [Test]
    public void Search_Or_AcrossTwoIndexedProperties()
    {
        var table = new RbTable<TestModelA>();
        var m1 = new TestModelA { V1 = 1, V2 = "a" };
        var m2 = new TestModelA { V1 = 2, V2 = "b" };
        var m3 = new TestModelA { V1 = 3, V2 = "c" };
        table.Add(m1); table.Add(m2); table.Add(m3);

        var result = table.Search(m => m.V1 == 1 || m.V2 == "c").ToArray();

        Assert.That(result, Is.EquivalentTo(new[] { m1, m3 }));
    }

    [Test]
    public void Search_Not_ExcludesMatchingItems()
    {
        var table = new RbTable<TestModelA>();
        var m1 = new TestModelA { V1 = 1, V2 = "a" };
        var m2 = new TestModelA { V1 = 2, V2 = "b" };
        table.Add(m1); table.Add(m2);

        var result = table.Search(m => !(m.V2 == "a")).ToArray();

        Assert.That(result, Is.EquivalentTo(new[] { m2 }));
    }

    [Test]
    public void Search_DuplicateIndexedValue_ReturnsBothModels()
    {
        var table = new RbTable<TestModelA>();
        var m1 = new TestModelA { V1 = 5, V2 = "x" };
        var m2 = new TestModelA { V1 = 5, V2 = "y" };
        table.Add(m1); table.Add(m2);

        var result = table.Search(m => m.V1 == 5).ToArray();

        Assert.That(result, Is.EquivalentTo(new[] { m1, m2 }));
    }

    [Test]
    public void Remove_UpdatesIndexesSoModelNoLongerMatches()
    {
        var table = new RbTable<TestModelA>();
        var m1 = new TestModelA { V1 = 1, V2 = "a" };
        var m2 = new TestModelA { V1 = 2, V2 = "b" };
        table.Add(m1); table.Add(m2);

        table.Remove(m1);
        var result = table.Search(m => m.V1 >= 0).ToArray();

        Assert.That(result, Is.EquivalentTo(new[] { m2 }));
    }

    [Test]
    public void Remove_LastModelWithSharedKey_KeyGroupGoesAwayEntirely()
    {
        // Регрессия на ветку "if (group.Models.Count == 0) _entries.Remove(group)"
        // внутри IndexedStore.Remove — если она сломается, тут повиснет старая группа
        // и Search продолжит (ошибочно) находить одну из уже удалённых моделей.
        var table = new RbTable<TestModelA>();
        var m1 = new TestModelA { V1 = 1, V2 = "same" };
        var m2 = new TestModelA { V1 = 2, V2 = "same" };
        table.Add(m1); table.Add(m2);

        table.Remove(m1);
        table.Remove(m2);

        var result = table.Search(m => m.V2 == "same").ToArray();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Search_OnUnindexedProperty_ThrowsInvalidOperationException()
    {
        // Текущее поведение: RbTable ищет индекс строго по PropertyInfo.Name,
        // так что запрос по непроиндексированному свойству обязан упасть,
        // а не тихо просканировать всё целиком.
        var table = new RbTable<ModelWithUnindexedField>();
        table.Add(new ModelWithUnindexedField { Id = 1, Description = "x" });

        Assert.Throws<InvalidOperationException>(() =>
            table.Search(m => m.Description == "x"));
    }
}
