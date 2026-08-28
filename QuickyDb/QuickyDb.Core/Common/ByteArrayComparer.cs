using System;
using System.Collections.Generic;

namespace QuickyDb.Core.Common
{
    public sealed class ByteArrayComparer : IComparer<byte[]>, IEqualityComparer<byte[]>
    {
        public static readonly ByteArrayComparer Instance = new ByteArrayComparer();

        public int Compare(byte[] a, byte[] b)
        {
            int len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++)
            {
                int cmp = a[i].CompareTo(b[i]);
                if (cmp != 0) return cmp;
            }
            return a.Length.CompareTo(b.Length);
        }

        public bool Equals(byte[] a, byte[] b) => Compare(a, b) == 0;

        public int GetHashCode(byte[] key)
        {
            unchecked
            {
                int hash = 17;
                foreach (var b in key) hash = hash * 31 + b;
                return hash;
            }
        }
    }
}
