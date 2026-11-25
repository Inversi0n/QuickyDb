using System;
using System.Collections.Generic;

namespace QuickyDb.Core.Common;

/// <summary>
/// <para>Compares and hashes byte arrays by their content.</para>
/// <para>Arrays are ordered lexicographically, and bytes are treated as unsigned values.</para>
/// <para>If one array is a prefix of the other, the shorter array is less.</para>
/// </summary>
public sealed class ByteArrayComparer : IComparer<byte[]>, IEqualityComparer<byte[]>
{
    /// <summary>
    /// Shared instance. The comparer has no state, so it can be used from several threads.
    /// </summary>
    public static readonly ByteArrayComparer Instance = new ByteArrayComparer();

    // Порядок "короткий префикс меньше длинного" совпадает с memcmp.
    // На нём держатся терминаторы составных ключей, PrefixScan и IncrementPrefix, поэтому менять его нельзя.
    /// <summary>
    /// <para>Compares two arrays byte by byte.</para>
    /// <para>Neither array may be null.</para>
    /// </summary>
    /// <param name="a">First array.</param>
    /// <param name="b">Second array.</param>
    /// <returns>
    /// A negative number if <paramref name="a"/> is less than <paramref name="b"/>, zero if they are equal, a positive number if <paramref name="a"/> is greater.
    /// </returns>
    public int Compare(byte[] a, byte[] b)
    {
        int len = Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            int cmp = a[i].CompareTo(b[i]);
            if (cmp != 0)
            {
                return cmp;
            }
        }
        return a.Length.CompareTo(b.Length);
    }

    /// <summary>
    /// <para>Determines whether two arrays have the same length and the same bytes.</para>
    /// <para>Neither array may be null.</para>
    /// </summary>
    /// <param name="a">First array.</param>
    /// <param name="b">Second array.</param>
    /// <returns>True if the arrays are equal.</returns>
    public bool Equals(byte[] a, byte[] b) => Compare(a, b) == 0;

    /// <summary>
    /// <para>Returns a hash code computed from all bytes of the array.</para>
    /// <para>Arrays with equal content have equal hash codes. The array must not be null.</para>
    /// </summary>
    /// <param name="key">Array to hash.</param>
    /// <returns>Hash code of the array content.</returns>
    public int GetHashCode(byte[] key)
    {
        unchecked
        {
            int hash = 17;
            foreach (var b in key)
            {
                hash = hash * 31 + b;
            }
            return hash;
        }
    }
}
