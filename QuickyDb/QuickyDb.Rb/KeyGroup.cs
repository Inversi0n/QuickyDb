using System.Collections.Generic;

namespace QuickyDb.Rb;

/// <summary>
/// Models that are stored under the same index key.
/// </summary>
/// <typeparam name="TModel">Type of the model.</typeparam>
public sealed class KeyGroup<TModel>
{
    /// <summary>Models stored under the key.</summary>
    public readonly List<TModel> Models;

    /// <summary>Encoded index key shared by all models of the group.</summary>
    public readonly byte[] Key;

    /// <summary>
    /// Creates an empty group.
    /// </summary>
    /// <param name="key">Encoded index key of the group.</param>
    public KeyGroup(byte[] key)
    {
        Key = key;
        Models = new List<TModel>();
    }
}