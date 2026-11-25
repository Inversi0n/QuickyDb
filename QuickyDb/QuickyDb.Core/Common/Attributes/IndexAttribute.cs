using System;

namespace QuickyDb.Core.Common.Attributes;

/// <summary>
/// <para>Marks a property for indexing.</para>
/// <para>The table keeps a sorted index of the values of such a property, so queries can filter by it.</para>
/// <para>The type of the property must be supported by <see cref="ByteEncoder"/>.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IndexAttribute : Attribute
{
}