using QuickyDb.Core.Common;
using QuickyDb.Rb.IndexedStore;
using QuickyDb.Tests.RbTests.Models;

namespace QuickyDb.Tests.RbTests.IndexedStore;

[TestFixture]
internal class IndexedStoreTests
{
    [Test]
    public void PrefixScan_ReturnsOnlyKeysStartingWithPrefix()
    {
        var store = new CascadeIndexedStore<Person>(new[]
        {
            typeof(Person).GetProperty(nameof(Person.LastName)),
            typeof(Person).GetProperty(nameof(Person.FirstName))
        });

        var ivanovA = new Person { LastName = "Ivanov", FirstName = "Anna" };
        var ivanovB = new Person { LastName = "Ivanov", FirstName = "Boris" };
        var petrov = new Person { LastName = "Petrov", FirstName = "Anna" };

        store.Add(ivanovA);
        store.Add(ivanovB);
        store.Add(petrov);

        var prefix = ByteEncoder.EncodeComponent("Ivanov", isLastComponent: false);
        var result = store.PrefixScan(prefix).SelectMany(g => g).ToList();

        Assert.That(2, Is.EqualTo(result.Count));
        Assert.That(result.Contains(ivanovA), "Doesn't contains iavanov A");
        Assert.That(result.Contains(ivanovB), "Doesn't contains iavanov b");
        Assert.That(!result.Contains(petrov), "Should not Contains iavanov b");
    }

    [Test]
    public void PrefixScan_HandlesAllFF_LastByteEdgeCase()
    {
        // ключ, чей последний значащий байт префикса — 0xFF, IncrementPrefix должен
        // откатиться на предыдущий байт, а не вернуть некорректную границу
    }
}
