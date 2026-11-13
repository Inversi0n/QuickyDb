using QuickyDb.Core.Common;
using QuickyDb.Rb.IndexedStore;
using QuickyDb.Tests.RbTests.Models;

namespace QuickyDb.Tests.Core.Common;

[TestFixture]
internal class ByteEncoderTests
{
    private sealed class Item
    {
        public int A { get; set; }
        public string B { get; set; } = "";
    }

    private static byte[] Bytes(params byte[] bytes) => bytes;

    private static int Cmp(byte[] a, byte[] b)
        => Math.Sign(ByteArrayComparer.Instance.Compare(a, b));

    // Значения передаются от меньшего к большему. Проверяется, что закодированные
    // байты идут в том же порядке, причем строго (без равных соседей).
    private static void AssertStrictlyAscending<T>(params T[] ascending) where T : notnull
    {
        for (int i = 1; i < ascending.Length; i++)
        {
            var previous = ByteEncoder.Encode(ascending[i - 1]);
            var current = ByteEncoder.Encode(ascending[i]);
            Assert.That(Cmp(previous, current), Is.EqualTo(-1),
                $"{typeof(T).Name}: {ascending[i - 1]} должно кодироваться меньше, чем {ascending[i]}");
        }
    }

    private static CascadeIndexedStore<TModel> CreateStore<TModel>(params string[] propertyNames)
        => new CascadeIndexedStore<TModel>(
            propertyNames.Select(n => typeof(TModel).GetProperty(n)!).ToArray());

    private static List<TModel> InKeyOrder<TModel>(CascadeIndexedStore<TModel> store)
        => store.AllExcept(Array.Empty<byte>()).SelectMany(g => g).ToList();

    private static void AssertOrderingPreserved<T>(T[] values, Func<T, byte[]> encode)
    where T : IComparable<T>
    {
        var expectedOrder = values.OrderBy(v => v).ToArray();
        var actualOrder = values
            .OrderBy(v => encode(v), ByteArrayComparer.Instance)
            .ToArray();

        Assert.That(actualOrder, Is.EqualTo(expectedOrder));
    }

    // ---------- ширина ----------

    [TestCase((short)1, 2)]
    [TestCase((ushort)1, 2)]
    [TestCase(1, 4)]
    [TestCase(1u, 4)]
    [TestCase(1L, 8)]
    [TestCase(1UL, 8)]
    [TestCase(1.5f, 4)]
    [TestCase(1.5, 8)]
    [TestCase((byte)1, 1)]
    [TestCase(true, 1)]
    public void Encode_ProducesFixedWidth(object value, int expectedWidth)
    {
        Assert.That(ByteEncoder.Encode(value), Has.Length.EqualTo(expectedWidth));
    }

    [Test]
    public void Encode_DateTime_IsEightBytes()
    {
        Assert.That(ByteEncoder.Encode(new DateTime(2000, 1, 1)), Has.Length.EqualTo(8));
    }

    // ---------- порядок байтов совпадает с порядком значений ----------

    [Test]
    public void Order_Int16()
        => AssertStrictlyAscending<short>(short.MinValue, -1000, -1, 0, 1, 1000, short.MaxValue);

    [Test]
    public void Order_UInt16()
        => AssertStrictlyAscending<ushort>(0, 1, 255, 256, 0x7FFF, 0x8000, ushort.MaxValue);

    [Test]
    public void Order_Int32()
        => AssertStrictlyAscending<int>(int.MinValue, -1000, -1, 0, 1, 255, 256, int.MaxValue);

    [Test]
    public void Order_UInt32()
        => AssertStrictlyAscending<uint>(0u, 1u, 0x7FFFFFFFu, 0x80000000u, 3000000000u, uint.MaxValue);

    [Test]
    public void Order_Int64()
        => AssertStrictlyAscending<long>(long.MinValue, -1000, -1, 0, 1, long.MaxValue);

    [Test]
    public void Order_UInt64()
        => AssertStrictlyAscending<ulong>(0UL, 1UL, long.MaxValue, 0x8000000000000000UL, ulong.MaxValue);

    [Test]
    public void Order_Single()
        => AssertStrictlyAscending<float>(
            float.NegativeInfinity, float.MinValue, -1000.5f, -1f, -float.Epsilon,
            0f,
            float.Epsilon, 1f, 1000.5f, float.MaxValue, float.PositiveInfinity);

    [Test]
    public void Order_Double()
        => AssertStrictlyAscending<double>(
            double.NegativeInfinity, double.MinValue, -1000.5, -1.0, -double.Epsilon,
            0.0,
            double.Epsilon, 1.0, 1000.5, double.MaxValue, double.PositiveInfinity);

    [Test]
    public void Order_DateTime()
        => AssertStrictlyAscending<DateTime>(
            DateTime.MinValue,
            new DateTime(1970, 1, 1),
            new DateTime(2000, 1, 1),
            new DateTime(2000, 1, 1, 0, 0, 1),
            DateTime.MaxValue);

    // ---------- эталонные байты ----------

    [Test]
    public void Encode_Integers_ProduceExpectedBytes()
    {
        Assert.Multiple(() =>
        {
            // знаковые: знаковый бит перевернут
            Assert.That(ByteEncoder.Encode((short)0), Is.EqualTo(Bytes(0x80, 0x00)));
            Assert.That(ByteEncoder.Encode((short)-1), Is.EqualTo(Bytes(0x7F, 0xFF)));
            Assert.That(ByteEncoder.Encode((short)1), Is.EqualTo(Bytes(0x80, 0x01)));
            Assert.That(ByteEncoder.Encode(short.MinValue), Is.EqualTo(Bytes(0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(short.MaxValue), Is.EqualTo(Bytes(0xFF, 0xFF)));

            Assert.That(ByteEncoder.Encode(0), Is.EqualTo(Bytes(0x80, 0x00, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(-1), Is.EqualTo(Bytes(0x7F, 0xFF, 0xFF, 0xFF)));
            Assert.That(ByteEncoder.Encode(255), Is.EqualTo(Bytes(0x80, 0x00, 0x00, 0xFF)));
            Assert.That(ByteEncoder.Encode(256), Is.EqualTo(Bytes(0x80, 0x00, 0x01, 0x00)));
            Assert.That(ByteEncoder.Encode(int.MinValue), Is.EqualTo(Bytes(0x00, 0x00, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(int.MaxValue), Is.EqualTo(Bytes(0xFF, 0xFF, 0xFF, 0xFF)));

            Assert.That(ByteEncoder.Encode(0L),
                Is.EqualTo(Bytes(0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(-1L),
                Is.EqualTo(Bytes(0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)));

            // беззнаковые: значение записано как есть, big-endian
            Assert.That(ByteEncoder.Encode((ushort)0), Is.EqualTo(Bytes(0x00, 0x00)));
            Assert.That(ByteEncoder.Encode((ushort)0x1234), Is.EqualTo(Bytes(0x12, 0x34)));
            Assert.That(ByteEncoder.Encode(ushort.MaxValue), Is.EqualTo(Bytes(0xFF, 0xFF)));

            Assert.That(ByteEncoder.Encode(0u), Is.EqualTo(Bytes(0x00, 0x00, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(0x01020304u), Is.EqualTo(Bytes(0x01, 0x02, 0x03, 0x04)));
            Assert.That(ByteEncoder.Encode(uint.MaxValue), Is.EqualTo(Bytes(0xFF, 0xFF, 0xFF, 0xFF)));

            Assert.That(ByteEncoder.Encode(0x0102030405060708UL),
                Is.EqualTo(Bytes(0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08)));
        });
    }

    [Test]
    public void Encode_FloatingPoint_ProducesExpectedBytes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ByteEncoder.Encode(0f), Is.EqualTo(Bytes(0x80, 0x00, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(1f), Is.EqualTo(Bytes(0xBF, 0x80, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(-1f), Is.EqualTo(Bytes(0x40, 0x7F, 0xFF, 0xFF)));

            Assert.That(ByteEncoder.Encode(0.0),
                Is.EqualTo(Bytes(0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(1.0),
                Is.EqualTo(Bytes(0xBF, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)));
            Assert.That(ByteEncoder.Encode(-1.0),
                Is.EqualTo(Bytes(0x40, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF)));
        });
    }

    [Test]
    public void Encode_NegativeZero_SortsBeforePositiveZero_KnownLimitation()
    {
        // Фиксирует текущее поведение, о котором говорит TODO в начале ByteEncoder:
        // -0.0 и +0.0 равны численно, но кодируются разными байтами.
        // Когда начнём приводить -0.0 к +0.0 в кодировщике, заменим оба Assert на проверку равенства.
        var negativeZeroSingle = BitConverter.Int32BitsToSingle(unchecked((int)0x80000000));
        var negativeZeroDouble = BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000));

        Assert.Multiple(() =>
        {
            Assert.That(Cmp(ByteEncoder.Encode(negativeZeroSingle), ByteEncoder.Encode(0f)), Is.EqualTo(-1));
            Assert.That(Cmp(ByteEncoder.Encode(negativeZeroDouble), ByteEncoder.Encode(0.0)), Is.EqualTo(-1));
        });
    }

    // ---------- прочие типы ----------

    [Test]
    public void Encode_StringBoolAndByte()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ByteEncoder.Encode("abc"), Is.EqualTo(Bytes(0x61, 0x62, 0x63)));
            Assert.That(ByteEncoder.Encode(false), Is.EqualTo(Bytes(0x00)));
            Assert.That(ByteEncoder.Encode(true), Is.EqualTo(Bytes(0x01)));
            Assert.That(ByteEncoder.Encode((byte)0xAB), Is.EqualTo(Bytes(0xAB)));
        });
    }

    [Test]
    public void Encode_UnsupportedType_Throws()
    {
        Assert.That(() => ByteEncoder.Encode(Guid.NewGuid()), Throws.TypeOf<NotSupportedException>());
    }

    // ---------- EncodeComponent ----------

    [Test]
    public void EncodeComponent_FixedWidth_IsSameAsEncode_ForAnyPosition()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ByteEncoder.EncodeComponent(42, isLastComponent: false),
                Is.EqualTo(ByteEncoder.Encode(42)));
            Assert.That(ByteEncoder.EncodeComponent(42, isLastComponent: true),
                Is.EqualTo(ByteEncoder.Encode(42)));
        });
    }

    [Test]
    public void EncodeComponent_String_LastComponent_HasNoTerminator()
    {
        Assert.That(ByteEncoder.EncodeComponent("abc", isLastComponent: true),
            Is.EqualTo(Bytes(0x61, 0x62, 0x63)));
    }

    [Test]
    public void EncodeComponent_String_NotLast_EndsWithTerminator()
    {
        Assert.That(ByteEncoder.EncodeComponent("Ivanov", isLastComponent: false),
            Is.EqualTo(Bytes(0x49, 0x76, 0x61, 0x6E, 0x6F, 0x76, 0x00, 0x00)));
    }

    [Test]
    public void EncodeComponent_String_EmbeddedNul_IsEscaped()
    {
        Assert.That(ByteEncoder.EncodeComponent("a\0b", isLastComponent: false),
            Is.EqualTo(Bytes(0x61, 0x00, 0xFF, 0x62, 0x00, 0x00)));
    }

    [Test]
    public void EncodeComponent_EmptyString()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ByteEncoder.EncodeComponent("", isLastComponent: false), Is.EqualTo(Bytes(0x00, 0x00)));
            Assert.That(ByteEncoder.EncodeComponent("", isLastComponent: true), Is.Empty);
        });
    }

    [Test]
    public void EncodeComponent_String_PreservesOrder_IncludingNul()
    {
        var a = ByteEncoder.EncodeComponent("a", isLastComponent: false);
        var aNul = ByteEncoder.EncodeComponent("a\0", isLastComponent: false);
        var aNulB = ByteEncoder.EncodeComponent("a\0b", isLastComponent: false);
        var ivanov = ByteEncoder.EncodeComponent("Иванов", isLastComponent: false);
        var petrov = ByteEncoder.EncodeComponent("Петров", isLastComponent: false);

        Assert.Multiple(() =>
        {
            Assert.That(Cmp(a, aNul), Is.EqualTo(-1));
            Assert.That(Cmp(aNul, aNulB), Is.EqualTo(-1));
            Assert.That(Cmp(ivanov, petrov), Is.EqualTo(-1));
        });
    }

    // ---------- составные ключи целиком (через CascadeIndexedStore) ----------

    [Test]
    public void CompositeKey_IntThenString_OrdersByFirstThenSecond()
    {
        var store = CreateStore<Item>(nameof(Item.A), nameof(Item.B));

        foreach (var (a, b) in new[]
        {
            (3, "b"), (-5, "b"), (int.MinValue, "x"), (3, "a"), (0, "z"), (-5, "a"), (3, "ab")
        })
        {
            store.Add(new Item { A = a, B = b });
        }

        var actual = InKeyOrder(store).Select(i => (i.A, i.B)).ToList();

        Assert.That(actual, Is.EqualTo(new List<(int, string)>
        {
            (int.MinValue, "x"), (-5, "a"), (-5, "b"), (0, "z"), (3, "a"), (3, "ab"), (3, "b")
        }));
    }

    [Test]
    public void CompositeKey_StringBoundary_DoesNotCollide()
    {
        // Без терминатора ("AB","C") и ("A","BC") дали бы одинаковые байты.
        var store = CreateStore<Person>(nameof(Person.LastName), nameof(Person.FirstName));

        store.Add(new Person { LastName = "AB", FirstName = "C" });
        store.Add(new Person { LastName = "A", FirstName = "BC" });

        var actual = InKeyOrder(store).Select(p => (p.LastName, p.FirstName)).ToList();

        Assert.That(actual, Is.EqualTo(new List<(string, string)> { ("A", "BC"), ("AB", "C") }));
    }

    [Test]
    public void CompositeKey_NulInFirstString_KeepsOrder()
    {
        var store = CreateStore<Person>(nameof(Person.LastName), nameof(Person.FirstName));

        store.Add(new Person { LastName = "a\0", FirstName = "a" });
        store.Add(new Person { LastName = "a", FirstName = "z" });

        var actual = InKeyOrder(store).Select(p => (p.LastName, p.FirstName)).ToList();

        Assert.That(actual, Is.EqualTo(new List<(string, string)> { ("a", "z"), ("a\0", "a") }));
    }
    //----
    [Test]
    public void EncodeInt32_PreservesNumericOrdering()
    {
        var values = new[] { int.MinValue, -1000000, -1, 0, 1, 42, 1000000, int.MaxValue };
        AssertOrderingPreserved(values, ByteEncoder.EncodeInt32);
    }

    [Test]
    public void EncodeInt64_PreservesNumericOrdering()
    {
        var values = new long[] { long.MinValue, -1000000000000, -1, 0, 1, 1000000000000, long.MaxValue };
        AssertOrderingPreserved(values, ByteEncoder.EncodeInt64);
    }

    [Test]
    public void EncodeInt32_EqualValues_ProduceEqualBytes()
    {
        var a = ByteEncoder.EncodeInt32(42);
        var b = ByteEncoder.EncodeInt32(42);
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Encode_ViaObjectOverload_MatchesTypedEncoder()
    {
        // Encode(object) должен для int давать тот же результат, что и прямой вызов EncodeInt32,
        // а не молча терять данные при боксинге.
        object boxed = 12345;
        Assert.That(ByteEncoder.Encode(boxed), Is.EqualTo(ByteEncoder.EncodeInt32(12345)));
    }

    [Test]
    public void Encode_StringValues_PreservesOrdinalOrdering()
    {
        var values = new[] { "apple", "banana", "cherry", "date" };
        var expected = values.OrderBy(s => s, StringComparer.Ordinal).ToArray();

        var actual = values
            .OrderBy(s => (byte[])ByteEncoder.Encode(s), ByteArrayComparer.Instance)
            .ToArray();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Encode_Bool_FalseSortsBeforeTrue()
    {
        var falseBytes = ByteEncoder.Encode(false);
        var trueBytes = ByteEncoder.Encode(true);

        Assert.That(ByteArrayComparer.Instance.Compare(falseBytes, trueBytes), Is.LessThan(0));
    }

    [Test]
    public void Encode_DateTime_PreservesChronologicalOrdering()
    {
        var earlier = new DateTime(2020, 1, 1);
        var later = new DateTime(2024, 6, 15);

        var earlierBytes = ByteEncoder.Encode(earlier);
        var laterBytes = ByteEncoder.Encode(later);

        Assert.That(ByteArrayComparer.Instance.Compare(earlierBytes, laterBytes), Is.LessThan(0));
    }

    [Test]
    public void Encode_UnsupportedType_ThrowsNotSupportedException()
    {
        // decimal сознательно не входит в switch — фиксируем текущее (ожидаемое) поведение.
        Assert.Throws<NotSupportedException>(() => ByteEncoder.Encode(3.14m));
    }
}