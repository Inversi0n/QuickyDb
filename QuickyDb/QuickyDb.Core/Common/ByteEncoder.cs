using System;
using System.Collections.Generic;
using System.Text;

namespace QuickyDb.Core.Common;

public static class ByteEncoder
{
    //TODO think about -0.0 and +0.0.
    //TODO Think about NaN/Infinity

    /// <summary>
    /// Convert a system object (property value) into byte[] data
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static byte[] Encode(object value)
    {
        switch (value)
        {
            case short s: return EncodeInt16(s);
            case ushort us: return EncodeInt16(us);
            case int i: return EncodeInt32(i);
            case uint ui: return EncodeInt32(ui);
            case long l: return EncodeInt64(l);
            case ulong ul: return EncodeInt64(ul);
            case float f: return EncodeSingle(f);
            case double d: return EncodeDouble(d);
            case string s: return Encoding.UTF8.GetBytes(s);
            case bool b: return new byte[] { (byte)(b ? 1 : 0) };
            case byte b: return new byte[] { b };
            case DateTime dt: return EncodeInt64(dt.Ticks);
            default:
                throw new NotSupportedException($"Нет байтового кодировщика для типа {value.GetType()}");
        }
    }

    // Кодирует один компонент составного ключа.
    // В отличие от Encode(value) для одиночного индекса, здесь variable-length
    // поля (строки) должны быть самоограничены внутри байтовой строки — иначе
    // ("AB","C") и ("A","BC") дадут одинаковые байты при конкатенации.
    /// <summary>
    /// Enconde ont component of composite index
    /// <para>Adds Terminate bytes for string value</para>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isLastComponent"></param>
    /// <returns></returns>
    public static byte[] EncodeComponent(object value, bool isLastComponent)
    {
        if (value is string s)
        {
            var raw = Encoding.UTF8.GetBytes(s);
            return isLastComponent ? raw : EscapeAndTerminate(raw);
        }

        // фикс-ширина (int и т.п.) уже самоограничена по построению — используем как есть
        return Encode(value);
    }

    private static byte[] EscapeAndTerminate(byte[] raw)
    {
        var buffer = new List<byte>(raw.Length + 2);
        foreach (var b in raw)
        {
            buffer.Add(b);
            if (b == 0x00) buffer.Add(0xFF); // эскейп нулевого байта, чтобы не спутать с терминатором
        }
        buffer.Add(0x00);
        buffer.Add(0x00); // терминатор конца компонента
        return buffer.ToArray();
    }


    public static byte[] EncodeInt16(ushort value)
    {
        ushort flipped = (ushort)(value ^ 0x8000);
        var bytes = BitConverter.GetBytes(flipped); // перегрузка для ushort -> 2 байта
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] EncodeInt16(short value)
        => EncodeInt16(unchecked((ushort)value));


    public static byte[] EncodeInt32(uint value)
    {
        uint flipped = unchecked(value) ^ 0x80000000u;
        var bytes = BitConverter.GetBytes(flipped);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
    public static byte[] EncodeInt32(int value)
         => EncodeInt32(unchecked((uint)value));

    public static byte[] EncodeInt64(ulong value)
    {
        ulong flipped = unchecked(value) ^ 0x8000000000000000ul;
        var bytes = BitConverter.GetBytes(flipped);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
    public static byte[] EncodeInt64(long value)
         => EncodeInt64(unchecked((ulong)value));

    public static byte[] EncodeSingle(float value)
    {
        var raw = BitConverter.GetBytes(value);
        int bits = BitConverter.ToInt32(raw, 0);
        uint asUint = unchecked((uint)bits);
        uint transformed = bits < 0 ? ~asUint : asUint ^ 0x80000000u;
        var bytes = BitConverter.GetBytes(transformed);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] EncodeDouble(double value)
    {
        var raw = BitConverter.GetBytes(value);
        long bits = BitConverter.ToInt64(raw, 0);
        ulong asUlong = unchecked((ulong)bits);
        ulong transformed = bits < 0 ? ~asUlong : asUlong ^ 0x8000000000000000ul;
        var bytes = BitConverter.GetBytes(transformed);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
}