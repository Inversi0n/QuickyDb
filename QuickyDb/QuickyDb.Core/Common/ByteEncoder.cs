using System;
using System.Collections.Generic;
using System.Text;

namespace QuickyDb.Core.Common;

//Comparing two encoded arrays with <see cref="ByteArrayComparer"/> gives the same result as comparing the values.
/// <summary>
/// <para>Encodes values into byte arrays that keep the order of the values.</para>
/// </summary>
public static class ByteEncoder
{
    // TODO: продумать -0.0 и +0.0.
    // TODO: продумать NaN и Infinity.

    /// <summary>
    /// <para>Encodes a value so that the byte order of the results matches the order of the values.</para>
    /// <para>
    /// 
    /// Supported types are <see cref="short"/>, <see cref="ushort"/>, <see cref="int"/>, <see cref="uint"/>, <see cref="long"/>, <see cref="ulong"/>, <see cref="float"/>, <see cref="double"/>, <see cref="string"/>, <see cref="bool"/>, <see cref="byte"/> and <see cref="DateTime"/>.
    /// </para>
    /// <para>A string is encoded as UTF-8 and ordered by byte values, not by culture rules.
    /// An empty string gives an empty array.
    /// A DateTime is encoded by its ticks, so its Kind is ignored.</para>
    /// </summary>
    /// <param name="value">Value to encode. Must not be null.</param>
    /// <returns>
    /// Encoded value. Numbers and DateTime take a fixed size of 2, 4 or 8 bytes, bool and byte take one byte,
    /// a string takes as many bytes as its UTF-8 form.
    /// </returns>
    /// <exception cref="NotSupportedException">The type of <paramref name="value"/> is not supported.</exception>
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

    // Составной ключ это склейка байтов компонентов. Строка переменной длины должна быть самоограничена,
    // иначе ("AB", "C") и ("A", "BC") дадут одинаковые байты. Типы фиксированной ширины самоограничены
    // по построению и идут как есть. Последнему компоненту терминатор не нужен: после него ничего нет.
    /// <summary>
    /// <para>Encodes one component of a composite index key.</para>
    /// <para>
    /// A string that is not the last component is escaped and ends with a terminator,
    /// so the concatenated key is unambiguous and keeps the order of the components.
    /// </para>
    /// <para>Values of other types are encoded exactly as in <see cref="Encode"/>.</para>
    /// </summary>
    /// <param name="value">Value of the component. Must not be null.</param>
    /// <param name="isLastComponent">
    /// True if no component follows this one in the key.
    /// Only a string depends on it: the last string is encoded as plain UTF-8, without escaping and terminator.
    /// </param>
    /// <returns>Encoded component.</returns>
    /// <exception cref="NotSupportedException">The type of <paramref name="value"/> is not supported.</exception>
    public static byte[] EncodeComponent(object value, bool isLastComponent)
    {
        if (value is string s)
        {
            byte[] raw = Encoding.UTF8.GetBytes(s);
            return isLastComponent ? raw : EscapeAndTerminate(raw);
        }

        return Encode(value);
    }

    // Нулевой байт внутри строки превращается в 00 FF, а в конце идёт терминатор 00 00. Терминатор меньше
    // любого продолжения строки: оно начинается либо с байта 01..FF, либо с экранированного 00 FF.
    // Поэтому более короткая строка всегда идёт раньше более длинной, и порядок значений сохраняется.
    /// <summary>
    /// Escapes zero bytes and appends the terminator 00 00, so the component ends in a recognizable place.
    /// </summary>
    /// <param name="raw">UTF-8 bytes of the string.</param>
    /// <returns>Escaped bytes with the terminator.</returns>
    private static byte[] EscapeAndTerminate(byte[] raw)
    {
        var buffer = new List<byte>(raw.Length + 2);
        foreach (var b in raw)
        {
            buffer.Add(b);
            if (b == 0x00)
            {
                buffer.Add(0xFF);
            }
        }
        buffer.Add(0x00);
        buffer.Add(0x00);
        return buffer.ToArray();
    }

    /// <summary>
    /// Encodes the value as 2 bytes, most significant byte first.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeInt16(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <summary>
    /// <para>Encodes the value as 2 bytes, most significant byte first.</para>
    /// <para>The sign bit is flipped, so negative values come before positive ones.</para>
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeInt16(short value)
    {
        var v = unchecked((ushort)((ushort)value ^ 0x8000));
        return EncodeInt16(v);
    }

    /// <summary>
    /// Encodes the value as 4 bytes, most significant byte first.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeInt32(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <summary>
    /// <para>Encodes the value as 4 bytes, most significant byte first.</para>
    /// <para>The sign bit is flipped, so negative values come before positive ones.</para>
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeInt32(int value)
        => EncodeInt32(unchecked((uint)value ^ 0x80000000u));

    /// <summary>
    /// Encodes the value as 8 bytes, most significant byte first.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeInt64(ulong value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <summary>
    /// <para>Encodes the value as 8 bytes, most significant byte first.</para>
    /// <para>The sign bit is flipped, so negative values come before positive ones.</para>
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeInt64(long value)
        => EncodeInt64(unchecked((ulong)value ^ 0x8000000000000000ul));

    /// <summary>
    /// <para>Encodes the value as 4 bytes that follow the numeric order of the values.</para>
    /// <para>Negative zero is encoded differently from positive zero and comes before it.</para>
    /// <para>The position of NaN in the order is not defined.</para>
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeSingle(float value)
    {
        byte[] raw = BitConverter.GetBytes(value);
        int bits = BitConverter.ToInt32(raw, 0);
        uint asUint = unchecked((uint)bits);

        // У положительных чисел достаточно перевернуть знаковый бит. У отрицательных инвертируются все биты:
        // чем больше модуль, тем меньше число, и порядок байтов должен это отразить.
        uint transformed = bits < 0 ? ~asUint : asUint ^ 0x80000000u;
        byte[] bytes = BitConverter.GetBytes(transformed);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    /// <summary>
    /// <para>Encodes the value as 8 bytes that follow the numeric order of the values.</para>
    /// <para>Negative zero is encoded differently from positive zero and comes before it.</para>
    /// <para>The position of NaN in the order is not defined.</para>
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Encoded value.</returns>
    public static byte[] EncodeDouble(double value)
    {
        byte[] raw = BitConverter.GetBytes(value);
        long bits = BitConverter.ToInt64(raw, 0);
        ulong asUlong = unchecked((ulong)bits);

        // Преобразование то же, что в EncodeSingle: положительным переворачивается знаковый бит,
        // у отрицательных инвертируются все биты.
        ulong transformed = bits < 0 ? ~asUlong : asUlong ^ 0x8000000000000000ul;
        byte[] bytes = BitConverter.GetBytes(transformed);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }
}