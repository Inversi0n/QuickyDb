using QuickyDb.Core.Common;

namespace QuickyDb.Tests.Core.Common;

[TestFixture]
public class ByteArrayComparerTests
{
    private static int Compare(byte[] a, byte[] b)
        => Math.Sign(ByteArrayComparer.Instance.Compare(a, b));

    [Test]
    public void ShorterPrefix_IsLess_ThanLongerArray()
    {
        var prefix = new byte[] { 1, 2 };
        var longer = new byte[] { 1, 2, 3 };

        Assert.That(Compare(prefix, longer), Is.EqualTo(-1));
        Assert.That(Compare(longer, prefix), Is.EqualTo(1));
    }

    [Test]
    public void Bytes_AreComparedAsUnsigned()
    {
        // 0xFF должен быть больше 0x01, а не отрицательным числом
        Assert.That(Compare(new byte[] { 0xFF }, new byte[] { 0x01 }), Is.EqualTo(1));
    }

    [Test]
    public void Content_Wins_OverLength()
    {
        // длинный массив с меньшим первым байтом всё равно меньше
        Assert.That(Compare(new byte[] { 1, 9, 9 }, new byte[] { 2 }), Is.EqualTo(-1));
    }

    [Test]
    public void EqualArrays_AreEqual()
    {
        Assert.That(Compare(new byte[] { 1, 2 }, new byte[] { 1, 2 }), Is.EqualTo(0));
    }
    //----
    [Test]
    public void Compare_EqualArrays_ReturnsZero()
    {
        var a = new byte[] { 1, 2, 3 };
        var b = new byte[] { 1, 2, 3 };
        Assert.That(ByteArrayComparer.Instance.Compare(a, b), Is.Zero);
    }

    [Test]
    public void Compare_FirstDifferingByteDeterminesOrder()
    {
        var a = new byte[] { 1, 5, 9 };
        var b = new byte[] { 1, 6, 0 };
        Assert.That(ByteArrayComparer.Instance.Compare(a, b), Is.LessThan(0));
    }

    [Test]
    public void Compare_ShorterPrefixSortsBeforeLongerArray()
    {
        // Важное свойство для понимания: этот компаратор сам по себе НЕ разруливает
        // неоднозначность составных ключей (тем и вызвана отдельная EncodeComponent
        // с терминатором в ветке composite) — здесь просто фиксируем его сырое поведение.
        var shorter = new byte[] { 1, 2 };
        var longer = new byte[] { 1, 2, 3 };

        Assert.That(ByteArrayComparer.Instance.Compare(shorter, longer), Is.LessThan(0));
        Assert.That(ByteArrayComparer.Instance.Compare(longer, shorter), Is.GreaterThan(0));
    }

    [Test]
    public void Equals_MatchesCompareZero()
    {
        var a = new byte[] { 7, 7, 7 };
        var b = new byte[] { 7, 7, 7 };
        Assert.That(ByteArrayComparer.Instance.Equals(a, b), Is.True);
    }

    [Test]
    public void GetHashCode_EqualContent_ProducesEqualHash()
    {
        var a = new byte[] { 3, 1, 4, 1, 5 };
        var b = new byte[] { 3, 1, 4, 1, 5 };
        Assert.That(ByteArrayComparer.Instance.GetHashCode(a), Is.EqualTo(ByteArrayComparer.Instance.GetHashCode(b)));
    }

    [Test]
    public void GetHashCode_DifferentContent_ProducesDifferentHash()
    {
        var a = new byte[] { 1, 2, 3 };
        var b = new byte[] { 9, 8, 7 };
        Assert.That(ByteArrayComparer.Instance.GetHashCode(a), Is.Not.EqualTo(ByteArrayComparer.Instance.GetHashCode(b)));
    }

    [Test]
    public void Compare_SortsByteArraysInSortedSetAsExpected()
    {
        // Это ровно то использование, ради которого компаратор существует:
        // SortedSet<KeyGroup<TModel>> в IndexedStore полагается именно на этот порядок.
        var sorted = new SortedSet<byte[]>(ByteArrayComparer.Instance)
        {
            new byte[] { 2 },
            new byte[] { 1 },
            new byte[] { 1, 0 },
        };

        var ordered = new List<byte[]>(sorted);
        Assert.That(ordered[0], Is.EqualTo(new byte[] { 1 }));
        Assert.That(ordered[1], Is.EqualTo(new byte[] { 1, 0 }));
        Assert.That(ordered[2], Is.EqualTo(new byte[] { 2 }));
    }
}
