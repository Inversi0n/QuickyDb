using QuickyDb.Core.Common;
using QuickyDb.Rb.IndexedStore;
using QuickyDb.Tests.RbTests.Models;
using System.Reflection;

namespace QuickyDb.Tests.RbTests;

// SimpleIndexedStore внутренний (internal), доступен тестам через InternalsVisibleTo
// в QuickyDb.Rb.csproj.
[TestFixture]
internal class SimpleIndexedStoreTests
{
    private static PropertyInfo V1Property => typeof(TestModelA).GetProperty(nameof(TestModelA.V1))!;

    [Test]
    public void TryGetEqual_ReturnsAllModelsSharingKey()
    {
        var store = new SimpleIndexedStore<TestModelA>(V1Property);
        var a = new TestModelA { V1 = 1, V2 = "a" };
        var b = new TestModelA { V1 = 1, V2 = "b" };
        var c = new TestModelA { V1 = 2, V2 = "c" };

        store.Add(a);
        store.Add(b);
        store.Add(c);

        var found = store.TryGetEqual(ByteEncoder.Encode(1), out var models);

        Assert.That(found, Is.True);
        Assert.That(models, Is.EquivalentTo(new[] { a, b }));
    }

    [Test]
    public void TryGetEqual_MissingKey_ReturnsFalse()
    {
        var store = new SimpleIndexedStore<TestModelA>(V1Property);
        store.Add(new TestModelA { V1 = 1, V2 = "a" });

        var found = store.TryGetEqual(ByteEncoder.Encode(99), out var models);

        Assert.That(found, Is.False);
        Assert.That(models, Is.Null);
    }

    [Test]
    public void Remove_LastModelInGroup_RemovesGroupEntirely()
    {
        var store = new SimpleIndexedStore<TestModelA>(V1Property);
        var a = new TestModelA { V1 = 1, V2 = "a" };
        var b = new TestModelA { V1 = 1, V2 = "b" };
        store.Add(a);
        store.Add(b);

        store.Remove(a);
        Assert.That(store.TryGetEqual(ByteEncoder.Encode(1), out var afterFirstRemoval), Is.True);
        Assert.That(afterFirstRemoval, Is.EquivalentTo(new[] { b }));

        store.Remove(b);
        Assert.That(store.TryGetEqual(ByteEncoder.Encode(1), out _), Is.False);
    }

    [Test]
    public void RangeUpTo_ExclusiveBound_ExcludesEqualKey()
    {
        var store = FilledStore(0, 3);

        var matched = store.RangeUpTo(ByteEncoder.Encode(2), inclusive: false)
            .SelectMany(g => g).Select(m => m.V1);

        Assert.That(matched, Is.EquivalentTo(new[] { 0, 1 }));
    }

    [Test]
    public void RangeUpTo_InclusiveBound_IncludesEqualKey()
    {
        var store = FilledStore(0, 3);

        var matched = store.RangeUpTo(ByteEncoder.Encode(2), inclusive: true)
            .SelectMany(g => g).Select(m => m.V1);

        Assert.That(matched, Is.EquivalentTo(new[] { 0, 1, 2 }));
    }

    [Test]
    public void RangeFrom_InclusiveBound_IncludesEqualKey()
    {
        var store = FilledStore(0, 3);

        var matched = store.RangeFrom(ByteEncoder.Encode(2), inclusive: true)
            .SelectMany(g => g).Select(m => m.V1);

        Assert.That(matched, Is.EquivalentTo(new[] { 2, 3 }));
    }

    [Test]
    public void RangeFrom_ExclusiveBound_ExcludesEqualKey()
    {
        var store = FilledStore(0, 3);

        var matched = store.RangeFrom(ByteEncoder.Encode(2), inclusive: false)
            .SelectMany(g => g).Select(m => m.V1);

        Assert.That(matched, Is.EquivalentTo(new[] { 3 }));
    }

    [Test]
    public void AllExcept_ExcludesOnlyMatchingKey()
    {
        var store = FilledStore(0, 2);

        var matched = store.AllExcept(ByteEncoder.Encode(1))
            .SelectMany(g => g).Select(m => m.V1);

        Assert.That(matched, Is.EquivalentTo(new[] { 0, 2 }));
    }

    [Test]
    public void EmptyStore_RangeAndAllExceptQueriesYieldNothing()
    {
        var store = new SimpleIndexedStore<TestModelA>(V1Property);

        Assert.That(store.RangeUpTo(ByteEncoder.Encode(5), true), Is.Empty);
        Assert.That(store.RangeFrom(ByteEncoder.Encode(5), true), Is.Empty);
        Assert.That(store.AllExcept(ByteEncoder.Encode(5)), Is.Empty);
    }

    private static SimpleIndexedStore<TestModelA> FilledStore(int fromInclusive, int toInclusive)
    {
        var store = new SimpleIndexedStore<TestModelA>(V1Property);
        for (int v = fromInclusive; v <= toInclusive; v++)
            store.Add(new TestModelA { V1 = v, V2 = v.ToString() });
        return store;
    }
}
