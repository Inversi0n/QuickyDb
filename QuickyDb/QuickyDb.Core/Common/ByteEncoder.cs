using System;
using System.Text;

namespace QuickyDb.Core.Common;

public static class ByteEncoder
{
    public static byte[] Encode(object value)
    {
        switch (value)
        {
            case int i: return EncodeInt32(i);
            case long l: return EncodeInt64(l);
            case string s: return Encoding.UTF8.GetBytes(s);
            case bool b: return new byte[] { (byte)(b ? 1 : 0) };
            case DateTime dt: return EncodeInt64(dt.Ticks);
            default:
                throw new NotSupportedException($"Нет байтового кодировщика для типа {value.GetType()}");
        }
    }

    public static byte[] EncodeInt32(int value)
    {
        uint flipped = unchecked((uint)value) ^ 0x80000000u;
        var bytes = BitConverter.GetBytes(flipped);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] EncodeInt64(long value)
    {
        ulong flipped = unchecked((ulong)value) ^ 0x8000000000000000ul;
        var bytes = BitConverter.GetBytes(flipped);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
}