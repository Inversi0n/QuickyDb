using QuickyDb.Core.Queries;

namespace QuickyDb.Tests.CoreTests;

// Тестируем QueryExecutor изолированно от реального IndexedStore: подсовываем
// свой набор делегатов (Strategy) поверх обычного List<Item>. Это проверяет
// именно логику разбора/комбинирования дерева предикатов (And = intersect,
// Or = union, Not = complement), а не корректность byte-индекса.
[TestFixture]
internal class QueryExecutorTests
{
    private class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private static List<Item> SampleItems() => new()
    {
        new Item { Id = 1, Name = "a" },
        new Item { Id = 2, Name = "b" },
        new Item { Id = 3, Name = "c" },
        new Item { Id = 4, Name = "d" },
        new Item { Id = 5, Name = "e" },
    };

    private static QueryExecutor<Item> BuildExecutor(List<Item> items)
    {
        IReadOnlyList<Item> Filter(Func<Item, bool> predicate) => items.Where(predicate).ToList();

        var ops = new IndexOperations<Item>
        {
            All = () => items,
            GetEqual = (prop, value) => Filter(i => Equals(prop.GetValue(i), value)),
            AllExcept = (prop, value) => new[] { Filter(i => !Equals(prop.GetValue(i), value)) },
            RangeUpTo = (prop, bound, inclusive) => new[]
            {
                Filter(i =>
                {
                    var cmp = ((IComparable)prop.GetValue(i)!).CompareTo(bound);
                    return inclusive ? cmp <= 0 : cmp < 0;
                })
            },
            RangeFrom = (prop, bound, inclusive) => new[]
            {
                Filter(i =>
                {
                    var cmp = ((IComparable)prop.GetValue(i)!).CompareTo(bound);
                    return inclusive ? cmp >= 0 : cmp > 0;
                })
            },
        };

        return new QueryExecutor<Item>(ops);
    }

    [Test]
    public void Search_Equal_ReturnsMatchingItem()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => i.Id == 3).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 3 }));
    }

    [Test]
    public void Search_NotEqual_ReturnsAllOthers()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => i.Id != 3).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 4, 5 }));
    }

    [Test]
    public void Search_LessThan_IsExclusiveOfBound()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => i.Id < 3).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Search_LessThanOrEqual_IsInclusiveOfBound()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => i.Id <= 3).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Search_GreaterThanOrEqual_IsInclusiveOfBound()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => i.Id >= 4).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 4, 5 }));
    }

    [Test]
    public void Search_And_IntersectsBothConditions()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => i.Id > 1 && i.Id < 5).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 2, 3, 4 }));
    }

    [Test]
    public void Search_Or_UnionsBothConditions()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => i.Id == 1 || i.Id == 5).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 1, 5 }));
    }

    [Test]
    public void Search_Not_ReturnsComplementAgainstAll()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => !(i.Id == 3)).Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 4, 5 }));
    }

    [Test]
    public void Search_NestedAndOr_EvaluatesRecursively()
    {
        var executor = BuildExecutor(SampleItems());
        var result = executor.Search(i => (i.Id > 1 && i.Id < 4) || i.Name == "e").Select(i => i.Id);
        Assert.That(result, Is.EquivalentTo(new[] { 2, 3, 5 }));
    }
}
