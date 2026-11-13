using QuickyDb.Core.Common;
using QuickyDb.Rb.IndexedStore;
using QuickyDb.Tests.RbTests.Models;

namespace QuickyDb.Tests.RbTests.IndexedStore;

[TestFixture]
internal class IndexedStoreTests
{
    // Модель с фиксированной шириной в первом компоненте. Нужна, чтобы собрать префиксы,
    // которые заканчиваются на 0xFF или целиком состоят из 0xFF (на строках так не получится).
    private sealed class Item
    {
        public int A { get; set; }
        public string B { get; set; } = "";
    }

    private static CascadeIndexedStore<Person> CreatePersonStore()
        => new CascadeIndexedStore<Person>(new[]
        {
            typeof(Person).GetProperty(nameof(Person.LastName))!,
            typeof(Person).GetProperty(nameof(Person.FirstName))!
        });

    private static CascadeIndexedStore<Item> CreateItemStore()
        => new CascadeIndexedStore<Item>(new[]
        {
            typeof(Item).GetProperty(nameof(Item.A))!,
            typeof(Item).GetProperty(nameof(Item.B))!
        });

    private static List<T> Scan<T>(IndexedStoreBase<T> store, byte[] prefix)
        => store.PrefixScan(prefix).SelectMany(g => g).ToList();

    private static byte[] LastNamePrefix(string lastName)
        => ByteEncoder.EncodeComponent(lastName, isLastComponent: false);

    [Test]
    public void PrefixScan_ReturnsOnlyKeysStartingWithPrefix()
    {
        var store = CreatePersonStore();

        var ivanovA = new Person { LastName = "Ivanov", FirstName = "Anna" };
        var ivanovB = new Person { LastName = "Ivanov", FirstName = "Boris" };
        var petrov = new Person { LastName = "Petrov", FirstName = "Anna" };

        store.Add(ivanovA);
        store.Add(ivanovB);
        store.Add(petrov);

        var result = Scan(store, LastNamePrefix("Ivanov"));

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Does.Contain(ivanovA));
        Assert.That(result, Does.Contain(ivanovB));
        Assert.That(result, Does.Not.Contain(petrov));
    }

    [Test]
    public void PrefixScan_DoesNotMatchLongerStringWithSameBeginning()
    {
        // "Ivan" это начало строки "Ivanov", но не префикс составного ключа:
        // терминатор после первого компонента должен их разделить.
        var store = CreatePersonStore();

        var ivan = new Person { LastName = "Ivan", FirstName = "Anna" };
        var ivanov = new Person { LastName = "Ivanov", FirstName = "Anna" };

        store.Add(ivan);
        store.Add(ivanov);

        var result = Scan(store, LastNamePrefix("Ivan"));

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result, Does.Contain(ivan));
    }

    [Test]
    public void PrefixScan_EmptyStore_ReturnsEmpty()
    {
        var store = CreatePersonStore();

        Assert.That(Scan(store, LastNamePrefix("Ivanov")), Is.Empty);
    }

    [Test]
    public void PrefixScan_PrefixSmallerThanAllKeys_ReturnsEmptyInsteadOfThrowing()
    {
        // Регрессия: раньше нижней границей подставлялся min, и GetViewBetween
        // бросал ArgumentException, когда min оказывался больше successor.
        var store = CreatePersonStore();
        store.Add(new Person { LastName = "Petrov", FirstName = "Anna" });

        List<Person> result = null!;
        Assert.DoesNotThrow(() => result = Scan(store, LastNamePrefix("Ivanov")));
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void PrefixScan_PrefixGreaterThanAllKeys_ReturnsEmpty()
    {
        var store = CreatePersonStore();
        store.Add(new Person { LastName = "Petrov", FirstName = "Anna" });

        Assert.That(Scan(store, LastNamePrefix("Sidorov")), Is.Empty);
    }

    [Test]
    public void PrefixScan_PrefixBetweenKeys_ReturnsEmpty()
    {
        var store = CreatePersonStore();
        store.Add(new Person { LastName = "Ivanov", FirstName = "Anna" });
        store.Add(new Person { LastName = "Petrov", FirstName = "Anna" });

        Assert.That(Scan(store, LastNamePrefix("Kuznetsov")), Is.Empty);
    }

    [Test]
    public void PrefixScan_HandlesAllFF_LastByteEdgeCase()
    {
        // int.MaxValue кодируется как FF FF FF FF: IncrementPrefix возвращает null,
        // верхней границей становится max, и в результат должны попасть только
        // ключи с этим первым компонентом.
        var store = CreateItemStore();

        var maxX = new Item { A = int.MaxValue, B = "x" };
        var maxY = new Item { A = int.MaxValue, B = "y" };
        var belowMax = new Item { A = int.MaxValue - 1, B = "x" };
        var zero = new Item { A = 0, B = "x" };

        store.Add(maxX);
        store.Add(maxY);
        store.Add(belowMax);
        store.Add(zero);

        var prefix = ByteEncoder.EncodeComponent(int.MaxValue, isLastComponent: false);
        Assert.That(prefix, Is.EqualTo(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));

        var result = Scan(store, prefix);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Does.Contain(maxX));
        Assert.That(result, Does.Contain(maxY));
    }

    [Test]
    public void PrefixScan_PrefixEndingWithFF_RollsBackToPreviousByte()
    {
        // 255 кодируется как 80 00 00 FF. Последний байт 0xFF нельзя увеличить,
        // поэтому successor равен 80 00 01 (укороченный). Ключ для 256 (80 00 01 00 ...)
        // обязан остаться за границей диапазона.
        var store = CreateItemStore();

        var a254 = new Item { A = 254, B = "x" };
        var a255x = new Item { A = 255, B = "x" };
        var a255y = new Item { A = 255, B = "y" };
        var a256 = new Item { A = 256, B = "x" };

        store.Add(a254);
        store.Add(a255x);
        store.Add(a255y);
        store.Add(a256);

        var prefix = ByteEncoder.EncodeComponent(255, isLastComponent: false);
        Assert.That(prefix, Is.EqualTo(new byte[] { 0x80, 0x00, 0x00, 0xFF }));

        var result = Scan(store, prefix);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Does.Contain(a255x));
        Assert.That(result, Does.Contain(a255y));
    }
}