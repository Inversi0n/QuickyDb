using System;

namespace QuickyDb.Core.Common.Attributes;

/// <summary>
/// <para>Marks a property as one component of a composite index.</para>
/// <para>Properties of one model with the same <see cref="Name"/> form one index.</para>
/// <para>The components are placed into the key in ascending order of <see cref="Order"/>.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CompositeIndexAttribute : Attribute
{
    /// <summary>
    /// Name of the composite index. Properties with the same name belong to the same index.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Position of the property in the composite key. A smaller value comes first.
    /// </summary>
    public uint Order { get; set; }

    /// <summary>
    /// Creates the attribute.
    /// </summary>
    /// <param name="name">Name of the composite index.</param>
    /// <param name="order">Position of the property in the composite key. A smaller value comes first.</param>
    public CompositeIndexAttribute(string name, uint order)
    {
        Name = name;
        Order = order;
    }
}